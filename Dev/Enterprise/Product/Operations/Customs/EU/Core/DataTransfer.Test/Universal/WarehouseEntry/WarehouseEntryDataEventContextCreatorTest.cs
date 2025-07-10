using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	sealed class WarehouseEntryDataEventContextCreatorTest : TestCaseWithFactory
	{
		[TestDate(2023, 12, 31)]
		public void TestEventContextValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB100031";
			declaration.JE_HouseBill = "HB1000460";
			declaration.JE_RL_NKFinalDestination = "CATOR";
			declaration.JE_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "CCCB", "US").PK;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B00001011";
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_TotalInnerPackages = 40;
			var entry1 = Factory.NewMoq<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(entry1.Object);
			entry1.Object.CH_MessageType = "TP1";
			entry1.Object.EntryNumber = "ENTNUM123";
			entry1.Setup(x => x.PackagesCount).Returns(78);
			entry1.Object.CH_CEI_Instruction = cei.PK;

			Factory.Save();
			var manager = entry1.Object.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
CarrierCode - CCCB
MBOLNumber - MB100031
MBOLDestinationUNLOCO - CATOR
HBOLNumber - HB1000460
HBOLDestinationUNLOCO - CATOR
EntryNumber - ENTNUM123
EntryNumberCountryOfIssue - LV
EntryNumberType - TP1
DeclarationReference - 3-B00001011
MessageType - TP1
ClearanceReferenceNumber - ENTNUM123
OuterPackQty - 78
InnerPackQty - 40".Trim(), eventContextValues);
		}
	}
}
