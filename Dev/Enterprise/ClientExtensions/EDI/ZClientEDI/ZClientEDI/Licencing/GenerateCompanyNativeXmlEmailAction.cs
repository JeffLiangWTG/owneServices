using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Licencing.Module
{
	public class GenerateCompanyNativeXmlEmailAction : BaseExportAndEmailAction
	{
		public void ExportAndEmail(EDIOrgHeader organisation)
		{
			if (!ConfirmLicenceGeneration(organisation))
			{
				return;
			}

			var factory = new BusinessObjectFactory();
			string nativeContent;
			var company = factory.New<GlbCompany>();

			using (((ITransactionParticipant)factory).BeginTransactionWithManager())
			{
				company.GC_Name = organisation.OH_FullName;
				company.GC_Code = GetUniqueCode(typeof(GlbBranch), GlbBranchSchema.GB_Code, factory);
				company.GC_Address1 = organisation.MainAddress.OA_Address1;
				company.GC_Address2 = organisation.MainAddress.OA_Address2;
				company.GC_City = organisation.MainAddress.OA_City.SubstringSafe(0, company.GC_CityInfo.MaxLength);
				company.GC_PostCode = organisation.MainAddress.OA_PostCode;
				company.GC_BusinessRegNo = organisation.LicenceTaxationRegNo.SubstringSafe(0, company.GC_BusinessRegNoInfo.MaxLength);
				company.GC_BusinessRegNo2 = organisation.LicenceBusinessRegNo.SubstringSafe(0, company.GC_BusinessRegNo2Info.MaxLength);
				company.GC_IsGSTCashBasis = organisation.LicCompany.LC_IsGSTCashBasis;
				company.GC_IsGSTRegistered = organisation.LicCompany.LC_IsGSTRegistered;
				company.GC_IsWHTRegistered = organisation.LicCompany.LC_IsWHTRegistered;
				company.GC_IsWHTCashBasis = organisation.LicCompany.LC_IsWHTCashBasis;
				company.GC_IsReciprocal = organisation.LicCompany.LC_IsReciprocal;
				company.GC_RX_NKLocalCurrency = RefCurrency.LoadFromCurrencyCode(company.Factory, organisation.LicCompany.LC_RX_NKCurrency).Code;
				company.GC_RN_NKCountryCode = organisation.MainAddress.OA_RN_NKCountryCode;

				var branch = company.Branches.AddNew();
				branch.GB_Address1 = organisation.MainAddress.OA_Address1;
				branch.GB_Address2 = organisation.MainAddress.OA_Address2;
				branch.GB_RL_NKHomePort = organisation.OH_RL_NKClosestPort;
				branch.GB_BranchName = organisation.OH_FullNameTruncated;
				branch.GB_State = organisation.MainAddress.OA_State;
				branch.GB_City = organisation.MainAddress.OA_City.SubstringSafe(0, branch.GB_CityInfo.MaxLength);
				branch.GB_PostCode = organisation.MainAddress.OA_PostCode;
				branch.GB_Fax = organisation.MainAddress.OA_Fax;
				branch.GB_RN_NKCountryCode = organisation.MainAddress.OA_RN_NKCountryCode;

				branch.GB_Code = GetUniqueCode(typeof(GlbBranch), GlbBranchSchema.GB_Code, factory);

				branch.GB_Phone = organisation.MainAddress.OA_Phone;
				branch.GB_Fax = organisation.MainAddress.OA_Fax;
				branch.GB_Email = organisation.MainAddress.OA_Email;
				branch.GB_WebAddress = organisation.MainWebURL.PU_URL.Length <= branch.GB_WebAddressInfo.MaxLength
					? organisation.MainWebURL.PU_URL
					: ZString.Empty;

				company.GC_State = organisation.MainAddress.OA_State;
				company.GC_Phone = organisation.MainAddress.OA_Phone;
				company.GC_Fax = organisation.MainAddress.OA_Fax;
				company.GC_Email = organisation.MainAddress.OA_Email;
				company.GC_WebAddress = organisation.MainWebURL.PU_URL;

				factory.Save();

				var definitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
				var converter = new BusinessObjectToEntityConverter { DefinitionFinder = definitionFinder };
				var xmlSerializer = new NativeXmlSerializer { Converter = converter };
				var dataStream = xmlSerializer.SerializeToStream(company);

				using (var reader = new StreamReader(dataStream, Encoding.UTF8))
				{
					nativeContent = reader.ReadToEnd();

					int index;
					const string startingTag = "<GeoLocation>";
					const string closingTag = "</GeoLocation>";
					while ((index = nativeContent.IndexOf(startingTag, StringComparison.OrdinalIgnoreCase)) != -1)
					{
						int index2 = nativeContent.IndexOf(closingTag, index, StringComparison.OrdinalIgnoreCase);
						if (index2 > index)
						{
							nativeContent = nativeContent.Remove(index, index2 - index + closingTag.Length);
						}
					}

					nativeContent = nativeContent.Replace(string.Format(CultureInfo.InvariantCulture, "<Code>{0}</Code>", company.GC_Code),
						string.Format(CultureInfo.InvariantCulture, "<Code>{0}</Code>", organisation.LicCompany.LC_CompanyCode));

					var branchCode = organisation.MainAddress.OA_RL_NKRelatedPortCode.SubstringSafe(2, 3).ToUpper().PadRight(3, ' ');
					nativeContent = nativeContent.Replace(string.Format(CultureInfo.InvariantCulture, "<Code>{0}</Code>", branch.GB_Code),
						string.Format(CultureInfo.InvariantCulture, "<Code>{0}</Code>", branchCode));
				}
			}

			if (!string.IsNullOrEmpty(nativeContent))
			{
				var directory = GetDirectoryAndSave();

				if (!string.IsNullOrEmpty(directory))
				{
					var fileName = string.Format(CultureInfo.InvariantCulture, "{0}{1}.xml", organisation.LicenceEnterpriseCode, organisation.LicCompany.LC_CompanyCode);
					var path = Path.Combine(directory, fileName);

					var error = SendOneEmailCompany(nativeContent, organisation, path);
					if (!string.IsNullOrEmpty(error))
					{
						Globals.Message.ShowError(error, Res.GetString("6BFE0764-76E1-47FD-8076-ACD812B957FE", "Error Occurred"));
					}
					else
					{
						Globals.Message.ShowInformation(
							Res.GetString("28EE71E8-B0AB-49B4-8D3C-D80758F24D7A",
								"The Company Export for {0} was successfully generated.\r\nThis file can be found at {1}", organisation.OH_Code,
								path), Res.GetString("F11DAC01-D5BA-4BC7-B090-B2513ED6A936", "Company Export Successful"));
					}
				}
			}
		}

		string GetDirectoryAndSave()
		{
			var directory = GetDirectory(EDIDataRegistry.Instance.CompanyExportDirectoryPath);
			if (!string.IsNullOrEmpty(directory))
			{
				EDIDataRegistry.Instance.CompanyExportDirectoryPath = directory;
			}

			return directory;
		}

		ZString GetUniqueCode(Type type, SchemaColumn column, BusinessObjectFactory factory)
		{
			ZString proposedCode = ZString.Empty;
			bool isUnique = false;
			while (!isUnique)
			{
				proposedCode = ZGuid.NewZGuid().ToString().Substring(0, 3);
				isUnique = factory.Load(type, new ZQuery(column, proposedCode)).Length == 0;
			}

			return proposedCode.ToUpperInvariant();
		}

		protected virtual string SendOneEmailCompany(string xmlContent, EDIOrgHeader organisation, string fullPath)
		{
			try
			{
				File.WriteAllText(fullPath, xmlContent);

				string subject = Res.GetString("A54C5123-8E1F-445E-AAF5-FAA212CFCAD3", "Company Native XML File for {0}", organisation.OH_FullNameTruncated);

				return SendEmail(organisation, subject, fullPath);
			}
			catch (UnauthorizedAccessException ex)
			{
				return Res.GetString("03CCE802-36B2-4501-AA31-E01162C700D8", "An error occurred while attempting to save the file: \r\n\r\n{0}", ex.Message);
			}
		}

		static bool ConfirmLicenceGeneration(EDIOrgHeader organisation)
		{
			var licCompany = organisation.LicCompany;

			if (licCompany == null)
			{
				return false;
			}

			string message = Res.GetString("A70BB755-B65A-47F0-BBE0-BE824A4B0222",
								 @"Please confirm you wish to generate an export key with the following information:
     Organization: {0}
     Exchange rates", organisation.OH_Code)
							 + " " + (
								 licCompany.LC_IsReciprocal
									 ? Res.GetString("e47c03a4-021b-40a1-886b-12caee4b384b", "are reciprocal")
									 : Res.GetString("a76b8bc0-37a2-47c1-a713-e7299c360534", "are not reciprocal")) + "\r\n     " +
							 Res.GetString("61362ff7-30b9-4d17-9af2-70e06f686f64", "GST Registered:") + " " +
							 (licCompany.LC_IsGSTRegistered
								 ? Res.GetString("bb0f5e96-c33f-4c67-aa77-f82898a3e664", "Yes")
								 : Res.GetString("d5ef4d42-a516-422a-ae36-bd1d5f9047b1", "No")) + "\r\n     " +
							 Res.GetString("a02ca7b6-7e5e-4148-85e8-dc7cc9399a4c", "GST Cash Basis:") + " " +
							 (licCompany.LC_IsGSTCashBasis
								 ? Res.GetString("bb0f5e96-c33f-4c67-aa77-f82898a3e664", "Yes")
								 : Res.GetString("d5ef4d42-a516-422a-ae36-bd1d5f9047b1", "No")) + "\r\n     " +
							 Res.GetString("e52fcd00-dce4-40ec-8fbf-fe3510229171", "WHT Registered:") + " " +
							 (licCompany.LC_IsWHTRegistered
								 ? Res.GetString("bb0f5e96-c33f-4c67-aa77-f82898a3e664", "Yes")
								 : Res.GetString("d5ef4d42-a516-422a-ae36-bd1d5f9047b1", "No")) + "\r\n     " +
							 Res.GetString("7eb5d6a3-015d-4ee5-8ebc-42d2978afd84", "WHT Cash Basis:") + " " +
							 (licCompany.LC_IsWHTCashBasis
								 ? Res.GetString("bb0f5e96-c33f-4c67-aa77-f82898a3e664", "Yes")
								 : Res.GetString("d5ef4d42-a516-422a-ae36-bd1d5f9047b1", "No")) + "\r\n     " +
							 Res.GetString("741ecae6-e2a1-480c-a47f-a94652177077",
								 "Currency: {0}\r\n     Taxation Reg No ({1}): {2}\r\n     Business Reg No ({3}): {4}",
								 licCompany.LC_RX_NKCurrency, organisation.LicenceTaxationRegNoType,
								 organisation.LicenceTaxationRegNo, organisation.LicenceBusinessRegNoType,
								 organisation.LicenceBusinessRegNo);

			return Globals.Message.Show(message,
					   Res.GetString("74F0F524-4E77-4718-A8F5-B1B2578612FF", "Are you sure you want to generate a Company export file from this Organization?"),
					   MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}
	}
}
