using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZCodeFindBoxFetchHintHandlerTest : TestCaseWithFactory
	{
		#region Setup

		protected virtual ZCodeFindBox GetNewFindBox()
		{
			return new ZCodeFindBox();
		}

		protected virtual ZCodeFindBoxFetchHintHandler GetNewFetcher(IBindToList control, IBusiness dataSource)
		{
			return new ZCodeFindBoxFetchHintHandler(control, dataSource);
		}

		#endregion

		public void TestAddFetchHintWithBusinessObjectFactoryIsNull()
		{
			using (var findBox = GetNewFindBox())
			{
				var dummy = new DummyBusinessObjectForNonFactory();
				findBox.BindTo = "Code";
				findBox.BindToList = "CodeTest";
				var fetchHandler = GetNewFetcher(findBox, dummy);

				AssertNoExceptionThrown(() => fetchHandler.Add());
			}
		}

		public void TestMore()
		{
			using (var findBox = GetNewFindBox())
			{
				var bizO = Factory.New<DummyDependantBusinessObject>();
				bizO.ZD1_Code = "CODE";
				bizO.ZD1_Z0 = ZGuid.NewZGuid();
				findBox.BindTo = GetBindToProperty();
				findBox.BindToList = "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett";
				AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));

				var fetchHandler = GetNewFetcher(findBox, bizO);
				fetchHandler.Add();
				AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			}
		}

		public void TestDeletedBizo()
		{
			using (var findBox = GetNewFindBox())
			{
				var bizO = Factory.New<DummyDependantBusinessObject>();
				bizO.ZD1_Code = "CODE";
				bizO.ZD1_Z0 = ZGuid.NewZGuid();
				findBox.BindTo = GetBindToProperty();
				findBox.BindToList = "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett";
				AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));

				bizO.Delete();
				var fetchHandler = GetNewFetcher(findBox, bizO);
				fetchHandler.Add();
				AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			}
		}

		protected virtual string GetBindToProperty()
		{
			return "ZD1_Code";
		}

		#region Implementation

		class DummyBusinessObjectForNonFactory : NonPersistentBusinessObject
		{
			public string Code { get; set; }

			DummyBusinessObjectCollection codeTest;
			public DummyBusinessObjectCollection CodeTest
			{
				get
				{
					if (codeTest == null)
					{
						codeTest = new DummyBusinessObjectCollection(Factory);
					}
					return codeTest;
				}
			}
		}

		#endregion
	}
}
