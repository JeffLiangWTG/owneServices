using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.DataTransfer.CusTempStorage.Testing
{
	class CusOutturnDataObjectWriterTest : OrganizationAddressTestHelper
	{
		[TestDate(2018, 3, 19, 12, 12, 12)]
		public void TestMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var startDate = ZDateTime.Today.AddDays(-2);
				var endDate = ZDateTime.Today.AddDays(2);
				var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
				helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Off1", "TestDescription", startDate,
					endDate);

				var job = Factory.New<CusTempStorageJobHeader>();
				job.SJH_JobReference = "SJH";
				job.SJH_OA_Presenter = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).MainAddress.PK;
				job.SJH_OH_Customer = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
				job.SJH_PresentationDate = new ZDate(2018, 3, 13);
				job.SJH_CustomsOffice = "Off1";
				job.SJH_ReferenceNumber = "123";
				job.SJH_PreviousReferenceType = "1";
				job.SJH_PreviousReferenceNumber = "2";
				job.SJH_TransportMode = "AIR";

				Factory.SaveForTesting();

				var writer = new CusTempStorageJobHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, job)));
				var jobData = writer.GetDataObject(job);

				CombineAssertions(() =>
				{
					AssertEquals("CustomsOffice.Code", "Off1", jobData.CustomsOffice.Code);
					AssertEquals("CustomsOffice.Description", "", jobData.CustomsOffice.Description);
					AssertDate("EntryDate", jobData, DateType.EntryDate, new ZDateTime(2018, 3, 13), true);
					AssertAdditionalReference(jobData.AdditionalReferenceCollection[0], "123",
						CodeDescriptionPairForTesting.New("TSR", "Temporary Storage Registration"), Core.Constants.CountryCodes.Germany);
					AssertAdditionalReference(jobData.AdditionalReferenceCollection[1], "2",
						CodeDescriptionPairForTesting.New("1", "Previous Reference Number"), Core.Constants.CountryCodes.Germany);
					AssertEquals("TransportMode.Code", "AIR", jobData.TransportMode.Code);
					AssertEquals("TransportMode.Description", "", jobData.TransportMode.Description);
					AssertNull("PackingLineCollection", jobData.PackingLineCollection);
					AssertNull("EntryHeaderCollection", jobData.EntryHeaderCollection);
				});

				AssertOrganizationBO_INTHEMSYD("SJH_OA_Presenter", jobData.OrganizationAddressCollection[0], Constants.AddressType.Declarant);
				AssertOrganizationBO_WUFSHIJNB("SJH_OH_Customer", jobData.OrganizationAddressCollection[1], Constants.AddressType.SendersLocalClient);
			}
		}

		public void TestMappings_Empty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var job = Factory.New<CusTempStorageJobHeader>();
				job.SJH_JobReference = "SJH";
				job.SJH_OH_Customer = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
				Factory.SaveForTesting();

				var writer = new CusTempStorageJobHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, job)));
				var jobData = writer.GetDataObject(job);

				CombineAssertions(() =>
				{
					AssertEquals("CustomsOffice.Code", "", jobData.CustomsOffice.Code);
					AssertEquals("CustomsOffice.Description", "", jobData.CustomsOffice.Description);
					AssertDate("EntryDate", jobData, DateType.EntryDate, ZDateTime.Empty, true);
					AssertAdditionalReference(jobData.AdditionalReferenceCollection[0], "",
						CodeDescriptionPairForTesting.New("TSR", "Temporary Storage Registration"), Core.Constants.CountryCodes.Germany);
					AssertAdditionalReference(jobData.AdditionalReferenceCollection[1], "",
						CodeDescriptionPairForTesting.New("", "Previous Reference Number"), Core.Constants.CountryCodes.Germany);
					AssertEquals("TransportMode.Code", "", jobData.TransportMode.Code);
					AssertEquals("TransportMode.Description", "", jobData.TransportMode.Description);
					AssertNull("PackingLineCollection", jobData.PackingLineCollection);
					AssertNull("EntryHeaderCollection", jobData.EntryHeaderCollection);
				});

				AssertOrganizationBO_WUFSHIJNB("SJH_OH_Customer", jobData.OrganizationAddressCollection[0], Constants.AddressType.SendersLocalClient);
			}
		}

		static void AssertDate(string message, UniversalShipment shipment, DateType dateType, ZDateTime expectedDate,
			bool expectedIsEstimate = false)
		{
			var date = shipment.DateCollection.First(x => x.Type.GetValueOrDefault() == dateType);
			AssertEquals(message, expectedDate, date.Value.GetValueOrDefault());
			AssertEquals(message, expectedIsEstimate, date.IsEstimate.GetValueOrDefault());
		}

		static void AssertAdditionalReference(AdditionalReference additionalReferenceDataObject, ZString referenceNumber,
			ICodeDescription type, string contextInformation = Core.Constants.CountryCodes.Italy)
		{
			AssertNotNull("Precondition: additionalReferenceDataObject", additionalReferenceDataObject);
			AssertEquals("additionalReferenceDataObject.ContextInformation", contextInformation, additionalReferenceDataObject.ContextInformation);
			AssertEquals("additionalReferenceDataObject.ReferenceNumber", referenceNumber, additionalReferenceDataObject.ReferenceNumber);
			AssertNotNull("additionalReferenceDataObject.Type", additionalReferenceDataObject.Type);
			AssertEquals("additionalReferenceDataObject.Type.Code", type.Code, additionalReferenceDataObject.Type.Code);
			AssertEquals("additionalReferenceDataObject.Type.Description", type.Description, additionalReferenceDataObject.Type.Description);
		}
	}
}
