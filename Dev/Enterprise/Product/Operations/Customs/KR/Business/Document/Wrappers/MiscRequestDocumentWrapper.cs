using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class MiscRequestDocumentWrapper : DocumentWrapper
	{
		public MiscRequestDocumentWrapper(CusMiscRequestHeader miscRequestHeader, BusinessObjectFactory factory)
			: base(miscRequestHeader, factory)
		{
			Argument.NotNull(miscRequestHeader, "entry");
			this.miscRequestHeader = miscRequestHeader;
		}

		readonly CusMiscRequestHeader miscRequestHeader;
		public CusMiscRequestHeader MiscRequestHeader => miscRequestHeader;

		public OrganizationDocWrapper Broker
		{
			get
			{
				if (broker == null && miscRequestHeader.Branch != null)
				{
					var branchOrgHeader = miscRequestHeader.Branch.OrgProxy;
					var companyOrgHeader = miscRequestHeader.Branch.Company?.OrgProxy;
					if (branchOrgHeader != null && branchOrgHeader.Addresses.ContainsAddressType(OrgAddressType.CustomsAddressOfRecord))
					{
						broker = new OrganizationDocWrapper(branchOrgHeader.CustomsAddress);
					}
					else if (companyOrgHeader != null)
					{
						broker = new OrganizationDocWrapper(companyOrgHeader.CustomsAddress ?? companyOrgHeader.MainAddress);
					}
				}
				return broker;
			}
		}
		OrganizationDocWrapper broker;

		public ZString Status => MiscRequestHeader.CMR_Status;
		public ZString StatusDescription => Factory.GetCachedValue<CustomsMessageStatusTypeList>().GetDescriptionFromCode(miscRequestHeader.CMR_Status);

		public ZString CustomsOfficeName
		{
			get
			{
				if (customsOfficeName.IsEmpty)
				{
					customsOfficeName = MessageFunctions.GetCustomsOffice(Factory, miscRequestHeader.CMR_CustomsOffice.SubstringSafe(0, 3));
				}
				return customsOfficeName;
			}
		}
		ZString customsOfficeName;

		public ZDateTime RequestDate => MiscRequestHeader.CMR_RequestDate;

		public ZString DepartmentName
		{
			get
			{
				if (departmentName.IsEmpty)
				{
					departmentName = MessageFunctions.GetCustomsDepartment(Factory, miscRequestHeader.CMR_CustomsOffice.SubstringSafe(3, 2));
				}
				return departmentName;
			}
		}
		ZString departmentName;

		public ZString FormattedApplicationNumber
		{
			get
			{
				if (formattedApplicationNumber.IsEmpty && miscRequestHeader.CusEntryNumber != null)
				{
					formattedApplicationNumber = MessageFunctions.DeclarationNumberFormat(miscRequestHeader.CusEntryNumber.CE_EntryNum);

					if (miscRequestHeader.CMR_MessageType == ElectronicDocumentTypeList.Codes._5GW)
					{
						var splitNumber = formattedApplicationNumber.Split(Constants.Hyphen);

						if (splitNumber.Length == 3)
						{
							formattedApplicationNumber = splitNumber[0] + Constants.ExtendedHoursRequestApplicationNumberText + splitNumber[1] + Constants.Hyphen + splitNumber[2];
						}
					}
				}

				return formattedApplicationNumber;
			}
		}
		ZString formattedApplicationNumber;

		public ExtendedHoursRequestHeader MessageData5AC
		{
			get
			{
				if (messageData5AC == null)
				{
					var message = (EDIMessage)miscRequestHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5AC);
					if (message != null)
					{
						messageData5AC = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, message.Branch.GB_GC);
						messageData5AC.PopulateFromMessage(message);
					}
				}
				return messageData5AC;
			}
		}
		ExtendedHoursRequestHeader messageData5AC;

		public ExtendedHoursRequestHeader MessageData5GW
		{
			get
			{
				if (messageData5GW == null)
				{
					var message = (EDIMessage)miscRequestHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5GW);
					if (message != null)
					{
						messageData5GW = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, message.Branch.GB_GC);
						messageData5GW.PopulateFromMessage(message);
					}
				}
				return messageData5GW;
			}
		}
		ExtendedHoursRequestHeader messageData5GW;

		public KREntryHeaderDetailsViewCollection ImportEntries
		{
			get
			{
				if (importEntries == null && miscRequestHeader.Branch != null)
				{
					var entryNumbers = MiscRequestHeader.RequestLines.Select(x => x.CML_EntryNumber);
					var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG(entryNumbers);
					importEntries = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, MiscRequestHeader.Branch.GB_GC);
				}
				return importEntries;
			}
		}
		KREntryHeaderDetailsViewCollection importEntries;

		public FinalPriceReportByDateExtensionHeader FinalPriceReportExtensionHeader
		{
			get
			{
				if (finalPriceReportExtensionHeader == null && MiscRequestHeader.CMR_MessageType == ElectronicDocumentTypeList.Codes._5SG)
				{
					var message = (EDIMessage)MiscRequestHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SG);
					if (message != null)
					{
						finalPriceReportExtensionHeader = new FinalPriceReportByDateExtensionHeader(Factory);
						finalPriceReportExtensionHeader.PopulateFromMessage(message);

						var incomingMessage = MiscRequestHeader.Messages.Cast<EDIMessage>()
												.FirstOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5SH && x.EM_ApplicationReference == message.EM_MessageNum);

						if (incomingMessage != null)
						{
							finalPriceReportExtensionHeader.PopulateFromIncomingMessage(incomingMessage);
						}

						if (ImportEntries != null)
						{
							finalPriceReportExtensionHeader.PopulateEntryData(ImportEntries);
						}
					}
				}
				return finalPriceReportExtensionHeader;
			}
		}
		FinalPriceReportByDateExtensionHeader finalPriceReportExtensionHeader;
	}
}
