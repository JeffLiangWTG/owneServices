using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class CMRCusUnderbondPluginTest : TestCaseWithFactory
	{
		public void TestCreateNewUserControl()
		{
			var dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			using (var testPlugIn = new CMRCusUnderbondPlugin(dummy, ZString.Empty))
			{
				using (ZForm testForm = new ZForm(dummy))
				{
					AssertType(typeof(AUCusUnderbondUserControl), testPlugIn.UserControl);
				}
			}
		}

		public void TestCMRCusUnderbondExportToCSVMenu()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			Business.CusUnderbondUnionCollectionParentCollection collection = new Business.CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (var testPlugIn = new CMRCusUnderbondPlugin(dummy, ZString.Empty))
			{
				using (ZForm testForm = new ZForm(dummy))
				{
					CusUnderbondUserControl control = testPlugIn.UserControl;
					using (control)
					{
						control.RemoveColumnsForAir();
						control.Parent = testForm;
						control.SetDataBinding(collection, "");
						testForm.Show();
						var contingencyMenuItem = control.UnderbondsGrid.ContextMenu.MenuItems.FindByText("Create Underbond Contingency Data");
						AssertNotNull("Context menu contains Contingency Data menu found", contingencyMenuItem);
						contingencyMenuItem.PerformClick();
						AssertEquals("Please select a row before attempting to create Contingency Data.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestSendUniversalXMLMenuItem()
		{
			var dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			var dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			var collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent))
			{ dummy };
			using (var testPlugIn = new CMRCusUnderbondPlugin(dummy, ZString.Empty))
			{
				using (var testForm = new ZForm(dummy))
				{
					using (var control = testPlugIn.UserControl)
					{
						control.RemoveColumnsForAir();
						control.Parent = testForm;
						control.SetDataBinding(collection, "");
						testForm.Show();
						var sendUniversalXMLMenuItem = control.UnderbondsGrid.ContextMenu.MenuItems.FindByText("Send Universal XML");
						AssertNotNull(sendUniversalXMLMenuItem);
						var universalShipmentMenuItem = sendUniversalXMLMenuItem.MenuItems.FindByText("Universal Shipment");
						AssertNotNull(universalShipmentMenuItem);
						universalShipmentMenuItem.PerformClick();
					}
				}
			}
		}

		sealed class DummyBizoWithUnderbondCollection : DummyBusinessObject, ICusUnderbondDependentCollectionParent
		{
			public DummyBizoWithUnderbondCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			CusUnderbondCollection underbonds;
			public Business.CusUnderbondCollection Underbonds
			{
				get
				{
					if (underbonds == null)
					{
						underbonds = new CusUnderbondCollectionWithProvider(this);
						underbonds.Load();
					}

					return underbonds;
				}
			}

			public IOutturnableLine[] OutturnableLines { get; set; } = Array.Empty<IOutturnableLine>();

			public bool UsesTranshipmentPortOnUnderbond
			{
				get => usesTranshipmentPortOnUnderbond;

				set => usesTranshipmentPortOnUnderbond = value;
			}

			public ZString DefaultTranshipmentPort
			{
				get => defaultTranshipmentPort;

				set => defaultTranshipmentPort = value;
			}

			ZString IOutturnableLine.UnderbondHumanReadableName => "Dummy Underbond Biz Obj";

			ZString ICusUnderbondDependentCollectionParent.Details => "Dummy Underbond Biz Obj Details";

			bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay => true;

			ZString IOutturnableLine.CargoStatus => "WTO";

			ZInt IOutturnableLine.PackagesManifested => 0;

			bool usesTranshipmentPortOnUnderbond;

			ZString defaultTranshipmentPort;
		}

		sealed class CusUnderbondCollectionWithProvider : CusUnderbondCollection
		{
			public CusUnderbondCollectionWithProvider(ICusUnderbondDependentCollectionParent master) : base(master)
			{
			}

			public new CusUnderBondWithParentLoader this[int i] => (CusUnderBondWithParentLoader)Elements[i];

			public new CusUnderBondWithParentLoader AddNew() => (CusUnderBondWithParentLoader)base.AddNew();

			public new CusUnderBondWithParentLoader AddNew(Type bizOType) => (CusUnderBondWithParentLoader)base.AddNew(bizOType);
		}

		sealed class CusUnderBondWithParentLoader : CusUnderbond
		{
			public CusUnderBondWithParentLoader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override TypeLoaderCollection GetParentLoaders()
			{
				var result = base.GetParentLoaders();
				result.Add(new TypeLoader(typeof(DummyBizoWithUnderbondCollection)));
				return result;
			}
		}

		sealed class DummyCusUnderbondUnionCollectionParent : DummyBusinessObject, IAUCusUnderbondUnionCollectionParent
		{
			public DummyCusUnderbondUnionCollectionParent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;
			CusUnderbondUnionCollection underbonds;
			public CusUnderbondUnionCollection AllUnderbonds
			{
				get
				{
					if (underbonds == null)
					{
						underbonds = new CusUnderbondUnionCollectionWithProvider(this);
						underbonds.Load();
					}

					return underbonds;
				}
			}

			public ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders() => AllPossibleCollectionProviders;

			public ICusUnderbondDependentCollectionParent[] AllPossibleCollectionProviders = Array.Empty<ICusUnderbondDependentCollectionParent>();

			public bool IsForAirCargo => false;
		}

		sealed class CusUnderbondUnionCollectionWithProvider : CusUnderbondUnionCollection
		{
			public CusUnderbondUnionCollectionWithProvider(IAUCusUnderbondUnionCollectionParent parent) : base(parent)
			{
			}

			public new CusUnderBondWithParentLoader this[int i] => (CusUnderBondWithParentLoader)Elements[i];

			public new CusUnderBondWithParentLoader AddNew() => (CusUnderBondWithParentLoader)base.AddNew();

			public new CusUnderBondWithParentLoader AddNew(Type bizOType) => (CusUnderBondWithParentLoader)base.AddNew(bizOType);
		}
	}
}
