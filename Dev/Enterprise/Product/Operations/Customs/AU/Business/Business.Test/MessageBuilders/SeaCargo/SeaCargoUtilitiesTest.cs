using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoUtilitiesTest : TestCaseWithFactory
	{
		public void TestAllFreightToCustomsConversionsHandled()
		{
			AssertEquals("Precondition: must return blank string for unknown code", string.Empty, SeaCargoUtilities.ConvertPkgUnitToCMRPackageType("___"));

			var errors = GetAllConversionErrors("Constants.PkgUnit", typeof(Core.Constants.PkgUnit), SeaCargoUtilities.ConvertPkgUnitToCMRPackageType);
			FailIfHasErrors("These codes are unhandled in converting from freight types to customs types: ", errors);
		}

		public void TestAllCustomsToFreightConversionsHandled()
		{
			AssertEquals("Precondition: must return blank string for unknown code", string.Empty, SeaCargoUtilities.ConvertCMRPackageTypeToPkgUnit("__"));

			var errors = GetAllConversionErrors("CMRPackageTypes.Codes", typeof(CMRPackageTypes.Codes), SeaCargoUtilities.ConvertCMRPackageTypeToPkgUnit);
			FailIfHasErrors("These codes are unhandled in converting from customs types to freight types: ", errors);
		}

		public void TestAllFreightToCustomsVolumeConversionsHandled()
		{
			AssertEquals("Precondition: must return same string for unknown code", "__", SeaCargoUtilities.ConvertVolumeUnitToCMRVolumeUnit("__"));
			AssertEquals("CubicMetres", "CU", SeaCargoUtilities.ConvertVolumeUnitToCMRVolumeUnit("M3"));
			AssertEquals("CubicMetres", "C8", SeaCargoUtilities.ConvertVolumeUnitToCMRVolumeUnit("D3"));
		}

		public void TestAllCustomsToFreightVolumeConversionsHandled()
		{
			AssertEquals("Must return M3 code for unknown unit", "M3", SeaCargoUtilities.ConvertCMRVolumeUnitToVolumeUnit("CM"));
			AssertEquals("CubicMetres", "M3", SeaCargoUtilities.ConvertCMRVolumeUnitToVolumeUnit("CU"));
			AssertEquals("CubicMetres", "D3", SeaCargoUtilities.ConvertCMRVolumeUnitToVolumeUnit("C8"));
		}

		public void TestWI00004668Rolls()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 5;
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Roll;

			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;

			var lCLContainer = consol.Containers.AddNew();
			lCLContainer.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			lCLContainer.JC_ContainerNum = "CHFU0303220";

			shipment.OuterPackLines[0].SetContainer(lCLContainer.PK);

			var synchroniser = new CMRSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.GetHouseBill(shipment);
			AssertEquals(1, houseBill.Pivot.Count);
			AssertEquals(CMRPackageTypes.Codes.Roll, houseBill.Pivot[0].CV_PackageType);
		}

		void FailIfHasErrors(string message, StringBuilder errors)
		{
			if (errors.Length > 0)
			{
				Fail(message + System.Environment.NewLine + errors.ToString());
			}
		}

		StringBuilder GetAllConversionErrors(string prefix, Type codesClass, ConversionDelegate conversionDelegate)
		{
			var result = new StringBuilder();

			foreach (var fieldInfo in codesClass.GetFields())
			{
				if (fieldInfo.FieldType == typeof(string))
				{
					if (conversionDelegate((string)fieldInfo.GetValue(null)) == string.Empty)
					{
						result.AppendFormat("case {0}.{1}:", prefix, fieldInfo.Name);
						result.AppendLine();
						result.AppendFormat("\tresult = {0};", fieldInfo.GetValue(null));
						result.AppendLine();
						result.AppendLine("\tbreak;");
					}
				}
			}

			return result;
		}
		delegate ZString ConversionDelegate(ZString code);
	}
}
