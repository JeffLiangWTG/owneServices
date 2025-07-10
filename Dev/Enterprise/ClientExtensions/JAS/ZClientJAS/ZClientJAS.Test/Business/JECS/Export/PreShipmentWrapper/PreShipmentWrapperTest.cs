using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	[TestedType(typeof(PreShipmentWrapper))]
	internal class PreShipmentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", Shipment, PreShipmentWrapper.Shipment);
			Assert("Should be registered as editable child object in the constructor", PreShipmentWrapper.IsRegisteredEditableChildObject(Shipment));
		}

		public void TestValidation()
		{
			AssertEquals(typeof(PreShipmentWrapperValidation), PreShipmentWrapper.Validation.GetType());
		}

		public void TestRunPreSaveValidation()
		{
			AssertEquals("Pre-condition", 0, PreShipmentWrapper.Notifications.Count());
			PreShipmentWrapper.RunPreSaveValidation();
			Assert(PreShipmentWrapper.SendingForwarderPKInfo.HasNotifications());
			Assert(PreShipmentWrapper.ReceivingForwarderPKInfo.HasNotifications());
		}

		public void TestHumanReadableName()
		{
			AssertEquals(Shipment.HumanReadableName, PreShipmentWrapper.HumanReadableName);
		}

		#region Properties
		public void TestValidationShouldNotBeCalledWhenSuspended()
		{
			using (PreShipmentWrapper.GetValidationSuspender())
			{
				PreShipmentWrapper.SendingForwarderPK = ZGuid.Empty;
				PreShipmentWrapper.ReceivingForwarderPK = ZGuid.Empty;
				Assert(!PreShipmentWrapper.HasNotifications());
			}
		}

		public void TestSendingForwarderPK()
		{
			ValidationHelper validationHelper = new ValidationHelper();
			ZGuid newGuid = ZGuid.NewZGuid();
			PreShipmentWrapper.SendingForwarderPK = newGuid;
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(PreShipmentWrapper.SendingForwarderPKInfo, true);
		}

		public void TestReceivingForwarderPK()
		{
			ZGuid newGuid = ZGuid.NewZGuid();
			PreShipmentWrapper.ReceivingForwarderPK = newGuid;
			PreShipmentWrapper.ReceivingForwarderPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, PreShipmentWrapper.ReceivingForwarderPK);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(PreShipmentWrapper.ReceivingForwarderPKInfo, true);
			PreShipmentWrapper.ReceivingForwarderPK = newGuid;
			AssertEquals(newGuid, PreShipmentWrapper.ReceivingForwarderPK);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(PreShipmentWrapper.ReceivingForwarderPKInfo, false);
		}

		public void TestForwarderList()
		{
			AssertEquals(typeof(ForwarderCollection), PreShipmentWrapper.SendingForwarders.GetType());
			AssertEquals(typeof(ForwarderCollection), PreShipmentWrapper.ReceivingForwarders.GetType());
		}

		#endregion
		#region IJXCExportHeader
		public void TestSendingForwarder()
		{
			AssertNull("Pre-condition", PreShipmentWrapper.SendingForwarder);
			PreShipmentWrapper.SendingForwarderPK = Factory.New(typeof(JASOrgHeader)).PK;
			AssertEquals(PreShipmentWrapper.SendingForwarderPK, PreShipmentWrapper.SendingForwarder.PK);
		}

		public void TestReceivingForwarder()
		{
			AssertNull("Pre-condition", PreShipmentWrapper.ReceivingForwarder);
			PreShipmentWrapper.ReceivingForwarderPK = Factory.New(typeof(JASOrgHeader)).PK;
			AssertEquals(PreShipmentWrapper.ReceivingForwarderPK, PreShipmentWrapper.ReceivingForwarder.PK);
		}

		public void TestFreightDest()
		{
			PreShipmentWrapper.Shipment.JS_RL_NKDestination = "IDJKT";
			AssertEquals("IDJKT", PreShipmentWrapper.FreightDest);
		}

		#endregion
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PreShipmentWrapper(Shipment);
		}

		PreShipmentWrapper PreShipmentWrapper
		{
			get
			{
				if (fPreShipmentWrapper == null)
				{
					fPreShipmentWrapper = new PreShipmentWrapper(Shipment);
				}

				return fPreShipmentWrapper;
			}
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
				}

				return fShipment;
			}
		}

		PreShipmentWrapper fPreShipmentWrapper;
		JASForwardingShipment fShipment;
		#endregion
	}
}
