using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5TECreator
	{
		public Import5TE Create(CusEntryHeader entry, string reason)
		{
			var import5TEData = new Import5TE();
			import5TEData.ImportDeclarationNumber = entry.EntryNumber;
			import5TEData.ApplicationReason = reason;
			import5TEData.BrokerID = entry.Declaration.UNIPASSDeclarantID;
			import5TEData.SequenceNo = entry.EntryNumbers?.Cast<CusEntryNumber>()?.Where(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5TE)?.Count() ?? ZInt.Zero;

			Populateimport5TEPayer(entry, import5TEData);

			return import5TEData;
		}

		void Populateimport5TEPayer(CusEntryHeader entry, Import5TE import5TEData)
		{
			if (entry.Declaration.PayerAddress != null)
			{
				import5TEData.Payer = new Organisation(RoleType.Payer)
				{
					IsIndividual = entry.Declaration.DutyPayer.GetIsIndividual()
				};

				var idNumbers = entry.Declaration.PayerAddress.GetRegistrationIDNumbers(IOrganizationExtensionMethods.GetBusinessOrIndividualRegistrationNumberTypes(entry.Declaration.DutyPayer.GetIsIndividual()));
				import5TEData.Payer.SetRegistrationIDNumbers(idNumbers);
			}
		}
	}
}
