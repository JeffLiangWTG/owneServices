using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EnableComplianceRiskDataType))]
	class EnableComplianceRiskDataTypeTest : RegistryDataTypeTestCase<EnableComplianceRiskDataType>
	{
		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

		public void TestValidation()
		{
			AssertValidation(false, true, true, true, @"The following module specific ComplianceWise registry settings need to be disabled in order to disable ComplianceWise on this system:
- Freight -> Compliance -> Enable ComplianceWise
- Liner & Agency -> Compliance -> Enable ComplianceWise
- ComplianceWise - Integrate ComplinaceWise to Customs Declaration module");
			AssertValidation(false, false, false, false, null);

			void AssertValidation(bool systemEnabled, bool freightEnabled, bool linerAgencyEnabled, bool customsEnabled, string message)
			{
				var freightRegistry = ComplianceWiseRegistryHelper.SetValue(freightEnabled);
				var linerAgencyRegistry = ComplianceWiseRegistryHelper.SetValue(linerAgencyEnabled);

				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(customsEnabled);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, freightRegistry))
				using (LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, linerAgencyRegistry))
				using (ObjectFactory.Substitute(featureControlMock.Object))
				{
					var dataType = new EnableComplianceRiskDataType();
					var registryItem = new BooleanRegistryItem("EnableComplianceRisk",
							null, null, null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							systemEnabled, dataType);

					if (!systemEnabled && (freightEnabled || linerAgencyEnabled || customsEnabled))
					{
						AssertExceptionThrown<RegistryValidationException>(message, () => dataType.Validate(registryItem, systemEnabled, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
					}
					else
					{
						AssertNoExceptionThrown(() => dataType.Validate(registryItem, systemEnabled, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
					}
				}
			}
		}

		public override void TestGetSetValidValues()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			{
				base.TestGetSetValidValues();
			}
		}

		protected override EnableComplianceRiskDataType GetNewDataType()
		{
			return new EnableComplianceRiskDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result = true;
			var result2 = false;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result, new BooleanRegistryDataType().Serialise(result)),
				new ValidSampleAndBinaryValueInDB(result2, new BooleanRegistryDataType().Serialise(result2))
			};
		}
	}
}
