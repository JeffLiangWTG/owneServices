using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	sealed class OperationalActionTestFieldSupporterListTest : TestCaseWithFactory
	{
		public void TestIndexer()
		{
			var list = new OperationalActionFieldSupporterList(typeof(DummyForActionWithARedefinedLink));
			OperationalActionFieldSupporter field1 = list["FollowChildren.Parent+Z0_AnotherDate"];
			AssertType("Expecting a datetime field supporter", typeof(OperationalActionDateTimeFieldSupporter), field1);
			OperationalActionFieldSupporter field2 = list["FollowChildren.Parent.Z0_AnotherDate"]; // should not throw exception
			AssertSame("Expecting the same field supporter despite being a slightly different representation of the same field.", field1, field2);
			OperationalActionFieldSupporter field3 = list["FollowChildren.Parent+Z0_DateTimeOffset"];
			AssertType("Expecting a datetimeoffset field supporter", typeof(OperationalActionDateTimeOffsetFieldSupporter), field3);
			OperationalActionFieldSupporter field4 = list["FollowChildren.Parent.Z0_DateTimeOffset"]; // should not throw exception
			AssertSame("Expecting the same field supporter despite being a slightly different representation of the same field.", field3, field4);
			OperationalActionFieldSupporter field5 = list["FollowChildren.Parent.Z0_Code"];
			AssertType("Expecting a code field supporter", typeof(OperationalActionCodeFieldSupporter), field5);
			OperationalActionFieldSupporter field6 = list["FollowChildren.Parent+Z0_Geography"];
			AssertType("Expecting a geography field supporter", typeof(OperationalActionGeographyFieldSupporter), field6);
			OperationalActionFieldSupporter field7 = list["FollowChildren.Parent.Z0_Geography"]; // should not throw exception
			AssertSame("Expecting the same field supporter despite being a slightly different representation of the same field.", field6, field7);
		}

		public void TestGetFieldType()
		{
			var list = new OperationalActionFieldSupporterList(typeof(DummyForActionWithARedefinedLink));
			Type type1 = list.GetFieldType("FollowChildren.Parent.Z0_Geography");
			AssertSame("Expecting the same field supporter despite being a slightly different representation of the same field.", typeof(ZGeography), type1);
			Type type2 = list.GetFieldType("FollowChildren");
			AssertSame("Expecting the \"FollowChildren\" field supporter", typeof(DummyChildCollectionForAction), type2);
		}

		public void TestBusinessObjectCollectionContainingBusinessObjectCollections()
		{
			var list = new OperationalActionFieldSupporterList(typeof(DummyWithWorkflow));
			OperationalActionFieldSupporter field1 = list["WorkflowItems.P9_ActualDateForBinding"];
			AssertType("Expecting a datetime field supporter", typeof(OperationalActionDateTimeOffsetFieldSupporter), field1);
			OperationalActionFieldSupporter field2 = list["WorkflowItems.Milestones.P9_ActualDateForBinding"];
			AssertType("Expecting a datetime field supporter", typeof(OperationalActionDateTimeOffsetFieldSupporter), field2);
			OperationalActionFieldSupporter field3 = list["WorkflowItems.Triggers.P9_ActualDateForBinding"];
			AssertType("Expecting a datetime field supporter", typeof(OperationalActionDateTimeOffsetFieldSupporter), field3);
			OperationalActionFieldSupporter field4 = list["WorkflowItems.Tasks.P9_ActualDateForBinding"];
			AssertType("Expecting a datetime field supporter", typeof(OperationalActionDateTimeOffsetFieldSupporter), field4);
		}

		#region HelperClasses
		internal interface IDummyForAction
		{
		}

		internal interface IDummyChildCollectionForAction
		{
		}

		#region DummyForAction
		internal class DummyForAction : DummyBusinessObject, IDummyForAction
		{
			public new class Schema : DummyBusinessObject.Schema
			{
				public const string Z0_RL_ParentHidden = "Z0_RL_ParentHidden";
				public const string Z0_RL_ParentShown = "Z0_RL_ParentShown";
				public const string Z0_RL_NKLoad = "Z0_RL_NKLoad";
				public const string Z0_OH_Consignor = "Z0_OH_Consignor";
				public const string Z0_NKOrigin = "Z0_NKOrigin";
				public const string ContactPK = "ContactPK";
			}

			public DummyForAction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ActionFieldFollow(typeof(IDummyForAction))]
			public BusinessObject ReturnTypeOverride
			{
				get
				{
					return null;
				}
			}

			[ActionFieldFollow(typeof(IDummyChildCollectionForAction))]
			public BusinessObjectCollection CollectionTypeOverride
			{
				get
				{
					return null;
				}
			}

			public ZString Z0_RL_ParentHidden
			{
				get
				{
					return "";
				}

				set
				{
				}
			}

			[ActionField]
			public ZString Z0_RL_ParentShown
			{
				get
				{
					return "";
				}

				set
				{
				}
			}

			public ZString Z0_RL_NKLoad
			{
				get
				{
					return "";
				}

				set
				{
				}
			}

			[ActionField(CollectionType = typeof(RefUNLOCOCollection))]
			public ZString Z0_NKOrigin
			{
				get
				{
					return "";
				}

				set
				{
				}
			}

			public ZGuid Z0_OH_Consignor
			{
				get
				{
					return ZGuid.Empty;
				}

				set
				{
				}
			}

			[ActionField(FieldType = ActionFieldType.Text, MaxLength = 35)]
			public ZString Z0_OA_TextFieldThatLooksLikeAModuleField
			{
				get
				{
					return ZString.Empty;
				}

				set
				{
				}
			}

			[ActionField(CollectionType = typeof(OrgContactCollection))]
			public ZGuid ContactPK
			{
				get
				{
					return ZGuid.Empty;
				}

				set
				{
				}
			}

			[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
			public override ZDateTime Z0_Date
			{
				get
				{
					return base.Z0_Date;
				}

				set
				{
					base.Z0_Date = value;
				}
			}

			[ActionField(LookUpEditType = OLookUpEditType.TransportType)]
			public override ZString Z0_Code
			{
				get
				{
					return base.Z0_Code;
				}

				set
				{
					base.Z0_Code = value;
				}
			}

			[ActionField(MaxLength = 100)]
			public override ZString Z0_VarCharMax
			{
				get
				{
					return base.Z0_VarCharMax;
				}

				set
				{
					base.Z0_VarCharMax = value;
				}
			}

			public DummyChildCollectionForAction FollowChildren
			{
				get
				{
					return new DummyChildCollectionForAction(Factory);
				}
			}

			[ActionFieldFollow(false)]
			public DummyChildCollectionForAction DontFollowChildren
			{
				get
				{
					return new DummyChildCollectionForAction(Factory);
				}
			}
		}

		#endregion
		#region DummyForActionWithARedefinedLink
		internal class DummyForActionWithARedefinedLink : DummyForAction
		{
			public DummyForActionWithARedefinedLink(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ActionFieldFollow(true)]
			public new DummyChildCollectionForAction FollowChildren
			{
				get
				{
					return base.FollowChildren;
				}
			}
		}

		#endregion
		#region DummyChildForActionBase
		internal class DummyChildForActionBase : DummyChildBusinessObject
		{
			public DummyChildForActionBase(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ActionFieldFollow]
			public DummyForAction Parent
			{
				get
				{
					return null;
				}
			}
		}

		#endregion
		#region DummyChildForAction
		internal class DummyChildForAction : DummyChildForActionBase
		{
			public DummyChildForAction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
			public override ZDateTime Z0_Date
			{
				get
				{
					return base.Z0_Date;
				}

				set
				{
					base.Z0_Date = value;
				}
			}

			[ActionField(LookUpEditType = OLookUpEditType.TransportType)]
			public override ZString Z0_Code
			{
				get
				{
					return base.Z0_Code;
				}

				set
				{
					base.Z0_Code = value;
				}
			}

			[ActionField(MaxLength = 100)]
			public override ZString Z0_VarCharMax
			{
				get
				{
					return base.Z0_VarCharMax;
				}

				set
				{
					base.Z0_VarCharMax = value;
				}
			}

			[ActionFieldFollow]
			public new DummyForActionWithARedefinedLink Parent
			{
				get
				{
					return null;
				}
			}
		}

		#endregion
		#region DummyChildCollectionForAction
		[ActionFieldFollow]
		internal class DummyChildCollectionForAction : DummyChildBusinessObjectCollection, IDummyChildCollectionForAction
		{
			public DummyChildCollectionForAction(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new DummyChildForAction this[int i]
			{
				get
				{
					return (DummyChildForAction)base[i];
				}
			}

			public new DummyChildForAction AddNew()
			{
				return (DummyChildForAction)base.AddNew();
			}
		}

		#endregion
		#endregion
	}
}
