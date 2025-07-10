using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class TransportMeansValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidateOfficeHasRequiredPurpose()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;

			foreach (var (data, code) in new[] { ("CUSOFF1", "DEP"), ("CUSOFF1", "DES"), ("CUSOFF2", "TRA"), ("CUSOFF3", "TXT"), ("CUSOFF4", "DEP") })
			{
				var office = departureMovement.CustomsOfficesForDeparture.AddNew();
				office.CY_Data = data;
				office.CY_Code = code;
			}

			var validOffices = new[] { "CUSOFF1", "CUSOFF2", "CUSOFF3" };
			var inValidOffices = new[] { "CUSOFF4", "CUSOFF5" };

			var dummyBO = Factory.New<DummyBusinessObject>();
			var propertyInfo = dummyBO.Z0_DescriptionInfo;

			using (dummyBO.SuspendValidationTesting())
			{
				CombineAssertions(() =>
				{
					foreach (ZString office in validOffices)
					{
						propertyInfo.ClearAllNotifications();
						propertyInfo.Value = office;
						TransportMeansValidationHelper.CheckOfficeHasRequiredPurpose(departureMovement, propertyInfo);
						AssertNoMessageErrors($"{office} is valid office", propertyInfo);
					}
					foreach (ZString office in inValidOffices)
					{
						propertyInfo.ClearAllNotifications();
						propertyInfo.Value = office;
						TransportMeansValidationHelper.CheckOfficeHasRequiredPurpose(departureMovement, propertyInfo);
						AssertHasMessageErrorContaining($"{office} is not valid office", propertyInfo, "Customs Office at Border must be equal");
					}
				});
			}
		}

		public void TestCheckReferenceNumber()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;

			var isMandatory = new[] { ("4", "40", "ENT"), ("4", "41", "ENT") };
			var notMandatory = new[] { ("4", "10", "ENT"), ("2", "40", "ENT"), ("4", "40", "NON") };

			var dummyBO = Factory.New<DummyBusinessObject>();
			var propertyInfo = dummyBO.Z0_DescriptionInfo;
			propertyInfo.Value = ZString.Empty;

			using (dummyBO.SuspendValidationTesting())
			{
				CombineAssertions(() =>
				{
					foreach (var (transportMode, idType, securityType) in notMandatory)
					{
						departureMovement.BM_ExportTransportMode = transportMode;
						departureMovement.BM_TypeOfSecurity = securityType;
						propertyInfo.ClearAllNotifications();
						TransportMeansValidationHelper.CheckReferenceNumber(departureMovement, idType, propertyInfo);
						AssertNoMessageErrors($"{transportMode} {idType} {securityType} is not mandatory", propertyInfo);
					}

					foreach (var (transportMode, idType, securityType) in isMandatory)
					{
						departureMovement.BM_ExportTransportMode = transportMode;
						departureMovement.BM_TypeOfSecurity = securityType;
						propertyInfo.ClearAllNotifications();
						propertyInfo.Value = ZString.Empty;
						TransportMeansValidationHelper.CheckReferenceNumber(departureMovement, idType, propertyInfo);
						AssertHasMessageErrors($"{transportMode} {idType} {securityType} is mandatory", propertyInfo);
						propertyInfo.Value = (ZString)"123";
						AssertNoMessageErrors($"{transportMode} {idType} {securityType} value entered", propertyInfo);
					}
				});
			}
		}
	}
}
