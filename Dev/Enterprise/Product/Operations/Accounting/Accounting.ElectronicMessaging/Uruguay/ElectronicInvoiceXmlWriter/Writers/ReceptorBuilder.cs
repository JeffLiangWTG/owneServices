using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public interface IReceptorBuilder
	{
		Receptor_Fact BuildEFacReceptor(TransactionInfo transaction);
		Receptor_Tck BuildETicketReceptor(TransactionInfo transaction);
	}

	class ReceptorBuilder : IReceptorBuilder
	{
		public ReceptorBuilder()
		{
			transactionInfoHelper_constructorInitializedOnly = new TransactionInfoHelper();
		}

		Receptor_Fact IReceptorBuilder.BuildEFacReceptor(TransactionInfo transaction)
		{
			var receptor = new Receptor_Fact();

			var country = transaction?.OrganizationAddress?.Country?.Code ?? ZString.Empty;
			var rut = TransactionInfoHelper.GetRegistrationCode(transaction?.OrganizationAddress, country, UruguayOrgCusCodeInfo.OrgCusCodes.RUT);

			if (country != CountryCodes.Uruguay || rut.IsNullOrEmpty())
			{
				return null;
			}
			else
			{
				receptor.TipoDocRecep = DocType.Item2;
				receptor.DocRecep = rut;
				receptor.CodPaisRecep = CodPaisType.UY;
			}

			(string rznSocRecep, string ciudadRecep, string dirRecep) = OrganizationAddressInfo(transaction);

			receptor.RznSocRecep = rznSocRecep;
			receptor.CiudadRecep = ciudadRecep;
			receptor.DirRecep = dirRecep;

			receptor.InfoAdicional = GetInfoAdicional(transaction?.OrganizationAddress);

			return receptor;
		}

		Receptor_Tck IReceptorBuilder.BuildETicketReceptor(TransactionInfo transaction)
		{
			var receptor = new Receptor_Tck();

			var country = transaction?.OrganizationAddress?.Country?.Code ?? ZString.Empty;

			if (!country.IsEmpty)
			{
				receptor.CodPaisRecepSpecified = true;
				receptor.CodPaisRecep = GetCountry(country);
				receptor.ItemElementName = (country == CountryCodes.Uruguay) ? ItemChoiceType.DocRecep : ItemChoiceType.DocRecepExt;
			}

			var registrationNumber = TransactionInfoHelper.GetRegistrationCode(transaction?.OrganizationAddress, country, UruguayOrgCusCodeInfo.OrgCusCodes.CID);
			if (!registrationNumber.IsNullOrEmpty())
			{
				receptor.TipoDocRecepSpecified = true;
				receptor.TipoDocRecep = DocType.Item3;
				registrationNumber = Regex.Replace(registrationNumber, @"[^0-9]", string.Empty);
			}
			else
			{
				registrationNumber = TransactionInfoHelper.GetRegistrationCode(transaction?.OrganizationAddress, country, OrgCusCode.CodeTypes.PassportID);
				if (!registrationNumber.IsNullOrEmpty())
				{
					receptor.TipoDocRecepSpecified = true;
					receptor.TipoDocRecep = DocType.Item5;
				}
				else
				{
					registrationNumber = transaction?.OrganizationAddress?.RegistrationNumberCollection?.FirstOrDefault(x => (x.Type?.Code).HasValue)?.Value;
					if (!registrationNumber.IsNullOrEmpty())
					{
						receptor.TipoDocRecepSpecified = true;
						receptor.TipoDocRecep = DocType.Item4;
					}
				}
			}

			receptor.Item = registrationNumber;

			(string rznSocRecep, string ciudadRecep, string dirRecep) = OrganizationAddressInfo(transaction);

			receptor.RznSocRecep = rznSocRecep;
			receptor.CiudadRecep = ciudadRecep;
			receptor.DirRecep = dirRecep;

			receptor.InfoAdicional = GetInfoAdicional(transaction?.OrganizationAddress);

			return receptor;
		}

		string GetInfoAdicional(OrganizationAddress organizationAddress)
		{
			var infoAdicional = TransactionInfoHelper.GetRegistrationCode(organizationAddress, CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.FZU);

			if (!infoAdicional.IsNullOrEmpty())
			{
				return "{ " + infoAdicional + " }";
			}

			return null;
		}

		(string rznSocRecep, string ciudadRecep, string dirRecep) OrganizationAddressInfo(TransactionInfo transaction)
		{
			var rznSocRecep = ZString.Empty;
			var ciudadRecep = ZString.Empty;
			var dirRecep = ZString.Empty;

			if (transaction?.OrganizationAddress != null)
			{
				if (transaction.OrganizationAddress.CompanyName.HasValue)
				{
					rznSocRecep = transaction.OrganizationAddress.CompanyName.Value.Substring(0, 150);
				}

				if (transaction.OrganizationAddress.City.HasValue)
				{
					ciudadRecep = transaction.OrganizationAddress.City.Value.Substring(0, 30);
				}

				if (transaction.OrganizationAddress.Address1.HasValue)
				{
					dirRecep = transaction.OrganizationAddress.Address1.Value.Substring(0, 70);
				}
			}

			return (rznSocRecep, ciudadRecep, dirRecep);
		}

		CodPaisType GetCountry(ZString countryCode)
		{
			return (countryCode == CountryCodes.NetherlandsAntilles || countryCode == CountryCodes.Kosovo || countryCode == "CS" || countryCode == "XZ" || !Enum.TryParse(countryCode, out CodPaisType country)) ? CodPaisType.Item99 : country;
		}

		ITransactionInfoHelper TransactionInfoHelper => transactionInfoHelper_constructorInitializedOnly;
		ITransactionInfoHelper transactionInfoHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteTransactionInfoHelper_ForTestOnly(ITransactionInfoHelper replacement) => transactionInfoHelper_constructorInitializedOnly = replacement;
		public ITransactionInfoHelper TransactionInfoHelper_ExposedForTestOnly => TransactionInfoHelper;
#endif
	}
}
