using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShipmentInspectionType))]
	sealed class ShipmentInspectionTypeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestIsIATAExemptionCode()
		{
			string[] invalidCodes = { ExemptionCodes.Codes.SmallUndersizedShipments, ExemptionCodes.Codes.Mail, ExemptionCodes.Codes.BiomedicalSamples, ExemptionCodes.Codes.DiplomaticBagsOrDiplomaticMail, ExemptionCodes.Codes.LifeSavingMaterials, ExemptionCodes.Codes.NuclearMaterial, ExemptionCodes.Codes.TransferOrTransshipment };
			string[] iATACodes = ShipmentInspectionType.GetIATAExemptionCodes();

			foreach (var code in iATACodes)
			{
				Assert(ShipmentInspectionType.IsIATAExemptionCode(code));
			}

			foreach (var code in invalidCodes)
			{
				Assert(!ShipmentInspectionType.IsIATAExemptionCode(code));
			}
		}

		public void TestCustomTypesWarning()
		{
			string message = "This is a custom inspection type. Custom inspection types, if not authorized by LGA, will cause rejection by airlines and may result in cargo being re-screened, delayed or not uplifted.";
			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value.Types;
			var systemDefinedType = inspectionTypes[0];
			systemDefinedType.ValidateCode();
			AssertNoWarning(systemDefinedType.CodeInfo, message);

			var userDefinedCode = inspectionTypes.AddNew();
			userDefinedCode.Code = "XZX";
			AssertHasWarning(userDefinedCode.CodeInfo, message);
		}

		public void TestGetIATAExemptionCode()
		{
			string[] cW1Codes = { ExemptionCodes.Codes.SmallUndersizedShipments, ExemptionCodes.Codes.Mail, ExemptionCodes.Codes.BiomedicalSamples, ExemptionCodes.Codes.DiplomaticBagsOrDiplomaticMail, ExemptionCodes.Codes.LifeSavingMaterials, ExemptionCodes.Codes.NuclearMaterial, ExemptionCodes.Codes.TransferOrTransshipment, ExemptionCodes.Codes.GovernmentApprovedReliableOrganization, ExemptionCodes.Codes.AdHocMovementsOfCargo };
			string[] iATACodes = ShipmentInspectionType.GetIATAExemptionCodes();

			for (int i = 0; i < cW1Codes.Length; i++)
			{
				AssertEquals(iATACodes[i], ShipmentInspectionType.GetIATAExemptionCode(cW1Codes[i]));
			}
		}

		public void TestCodeAndDescriptionReadOnly()
		{
			FallbackLevel systemFallBack = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			ShipmentInspectionType type = new ShipmentInspectionType(systemFallBack);
			AssertEquals(false, type.CodeInfo.ReadOnly);
			AssertEquals(false, type.DescriptionInfo.ReadOnly);
			AssertEquals(false, type.ShowInListInfo.ReadOnly);
			AssertEquals(false, type.AllowedOnPassengerFlightsInfo.ReadOnly);

			type = new ShipmentInspectionType();
			AssertEquals(false, type.CodeInfo.ReadOnly);
			AssertEquals(false, type.DescriptionInfo.ReadOnly);
			AssertEquals(false, type.ShowInListInfo.ReadOnly);
			AssertEquals(false, type.AllowedOnPassengerFlightsInfo.ReadOnly);
		}

		public void TestNewElementsCanBeDeleted()
		{
			FallbackLevel systemFallBack = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			ShipmentInspectionType type = new ShipmentInspectionType(systemFallBack);
			AssertEquals(true, type.CanDelete);

			type = new ShipmentInspectionType();
			AssertEquals(true, type.CanDelete);
		}

		public void TestUNKAndAPPStatusesValidation()
		{
			ShipmentInspectionType test = new ShipmentInspectionType();

			test.Code = "AAA";
			AssertNoErrors(test.CodeInfo);

			test.Code = "UNK";
			AssertHasError(test.CodeInfo, "UNK and APP inspection statuses are invalid for this registry.");

			test.Code = "APP";
			AssertHasError(test.CodeInfo, "UNK and APP inspection statuses are invalid for this registry.");
		}

		#region Implementation

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ShipmentInspectionType result = new ShipmentInspectionType();

			result.Code = "XRY";
			result.Description = (NoResString)"X-ray equipment";
			result.CodeMaxLength = 3;
			result.AllowedOnPassengerFlights = true;
			result.ShowInList = true;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
