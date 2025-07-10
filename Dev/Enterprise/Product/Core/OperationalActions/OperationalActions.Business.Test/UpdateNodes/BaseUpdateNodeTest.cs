using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public abstract class BaseUpdateNodeTest : TestCaseWithFactory
	{
		#region Implementation
		protected static OperationalActionTextFieldSupporter Field(SchemaStringColumn column, string name)
		{
			return Field("", column, name);
		}

		protected static OperationalActionDateTimeFieldSupporter Field(SchemaDateTimeColumn column, string name)
		{
			return Field("", column, name);
		}

		protected static OperationalActionDateTimeOffsetFieldSupporter Field(SchemaDateTimeOffsetColumn column, string name)
		{
			return Field("", column, name);
		}

		protected static OperationalActionTimeFieldSupporter Field(SchemaTimeColumn column, string name)
		{
			return Field("", column, name);
		}

		protected static OperationalActionGeographyFieldSupporter Field(SchemaGeographyColumn column, string name)
		{
			return Field("", column, name);
		}

		protected static OperationalActionTextFieldSupporter Field(string prefix, SchemaStringColumn column, string name)
		{
			return new OperationalActionTextFieldSupporter(prefix + column.Name, false, column.MaxLength);
		}

		protected static OperationalActionTextFieldSupporter Field(string prefix, string fieldName, int maxLength = 50)
		{
			return new OperationalActionTextFieldSupporter(prefix + fieldName, false, maxLength);
		}

		protected static OperationalActionDateTimeFieldSupporter Field(string prefix, SchemaDateTimeColumn column, string name)
		{
			return new OperationalActionDateTimeFieldSupporter(prefix + column.Name, false, Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short);
		}

		protected static OperationalActionDateTimeOffsetFieldSupporter Field(string prefix, SchemaDateTimeOffsetColumn column, string name)
		{
			return new OperationalActionDateTimeOffsetFieldSupporter(prefix + column.Name, false);
		}

		protected static OperationalActionTimeFieldSupporter Field(string prefix, SchemaTimeColumn column, string name)
		{
			return new OperationalActionTimeFieldSupporter(prefix + column.Name, false);
		}

		protected static OperationalActionGeographyFieldSupporter Field(string prefix, SchemaGeographyColumn column, string name)
		{
			return new OperationalActionGeographyFieldSupporter(prefix + column.Name, false);
		}

		protected DummyChildCollection NewDummyChildCollection(params DummyChild[] children)
		{
			DummyChildCollection collection = new DummyChildCollection(Factory);
			collection.AddRange(children);
			return collection;
		}

		List<Mock> mocks;
		protected Mock<BizObjT> New<BizObjT>(string name, MockBehavior mockBehavior = MockBehavior.Loose)
			where BizObjT : BusinessObject
		{
			var result = Factory.NewMoq<BizObjT>();
			result.Name = name;
			if (mocks == null)
			{
				mocks = new List<Mock>();
			}

			mocks.Add(result);
			return result;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (mocks != null)
			{
				foreach (var mock in mocks)
				{
					mock.VerifyAll();
				}
			}
		}

		#endregion
		#region Helper Classes
		#region DummyMaster
		[System.Diagnostics.DebuggerDisplay("Master, PK = {PK}")]
		public class DummyMaster : DummyBase
		{
			public DummyMaster(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			public virtual DummyChildCollection Children
			{
				get
				{
					return null;
				}
			}
		}

		#endregion

		#region DummyMasterWithMultiNestedCollections

		[ActionFieldFollow(true)]
		public class DummyMasterWithMultiNestedCollections : DummyBase
		{
			public DummyMasterWithMultiNestedCollections(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			[ActionFieldFollow(true)]
			public virtual DummyWithMultiNestedCollections Children
			{
				get
				{
					return null;
				}
			}

			[ActionFieldFollow(true)]
			public virtual DummyWithMultiNestedCollections MoreChildren
			{
				get
				{
					return null;
				}
			}
		}

		#endregion

		#region DummyChild
		[System.Diagnostics.DebuggerDisplay("Child, PK = {PK}")]
		public class DummyChild : DummyBase
		{
			public DummyChild(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			public virtual DummyMaster Master
			{
				get
				{
					return null;
				}
			}

			[ActionFieldFollow(true)]
			public virtual DummyWithAnotherNestedCollection ChildDummyCollection { get; set; }

			[ActionFieldFollow(true)]
			public DummyWithAnotherNestedCollection DummyDuplicateOne { get; set; }

			[ActionFieldFollow(true)]
			public DummyChildCollection DummyDuplicateTwo { get; set; }

			[ActionFieldFollow(true)]
			public DummyChild DummyDuplicateBizOOne { get; set; }

			[ActionFieldFollow(true)]
			public DummyChildCollection DummyFinalCollection { get; set; }
		}

		#endregion
		#region DummyBase
		[ActionFieldFollow(true)]
		public abstract class DummyBase : DummyBaseBusinessObject
		{
			public DummyBase(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			public sealed override bool Equals(object obj)
			{
				return base.Equals(obj);
			}

			public sealed override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public sealed override bool HasChanges
			{
				get
				{
					return base.HasChanges;
				}
				set
				{
					base.HasChanges = value;
				}
			}
		}

		#endregion
		#region DummyChildCollection
		[ActionFieldFollow(true)]
		public class DummyChildCollection : BusinessObjectCollection<DummyChild>
		{
			public DummyChildCollection(BusinessObjectFactory factory) : base(factory)
			{
			}

			[ActionFieldFollow(true)]
			public DummyWithMultiNestedCollections DummyDuplicateOne { get; set; }
		}

		#endregion

		#region Dummies With Nested Collections

		[ActionFieldFollow(true)]
		public class DummyWithMultiNestedCollections : BusinessObjectCollection<DummyChild>
		{
			[ActionFieldFollow(true)]
			public DummyWithAnotherNestedCollection ChildDummyCollectionOne { get; set; }

			[ActionFieldFollow(true)]
			public DummyChildCollection DummyDuplicateTwo { get; set; }

			[ActionFieldFollow(true)]
			public DummyChild DummyChildBizO { get; set; }

			public DummyWithMultiNestedCollections(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		[ActionFieldFollow(true)]
		public class DummyWithAnotherNestedCollection : BusinessObjectCollection<DummyChild>
		{
			[ActionFieldFollow(true)]
			public DummyChildCollection ChildDummyCollectionTwo { get; set; }

			public DummyWithAnotherNestedCollection(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		#endregion

		public class DummyWithReadOnly : DummyBusinessObject
		{
			public DummyWithReadOnly(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member Z0_Description_ReadOnly")]
			[ReadOnlyMember("Z0_Description_ReadOnly")]
			public override ZString Z0_Description
			{
				get
				{
					return base.Z0_Description;
				}

				set
				{
					base.Z0_Description = value;
				}
			}
		}

		public class DummyWithNoZPropertyInfo : DummyBusinessObject
		{
			public DummyWithNoZPropertyInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString Name { get; set; }
		}

		public class DummyWithListAttribute : DummyBusinessObject
		{
			public DummyWithListAttribute(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[List("Lookup")]
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

			public CodeDescriptionPairList Lookup
			{
				get
				{
					var result = new CodeDescriptionPairList();
					result.AddPair("AAA", "First");
					result.AddPair("BBB", "Second");
					return result;
				}
			}
		}
		#endregion
	}
}
