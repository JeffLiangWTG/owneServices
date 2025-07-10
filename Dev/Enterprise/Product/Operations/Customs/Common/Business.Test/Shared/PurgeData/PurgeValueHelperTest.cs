using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	sealed class PurgeValueHelperTest : BasePurgeHelperTest
	{
		[ExpectNoExceptions]
		public void TestGetAttributesFromBaseType()
		{
			var obj = Factory.New<DummyBoForAttribute>();
			var helper = new PurgeValueHelper<DummyBoForAttribute>(obj);
			var sourceInfos = helper.PurgeSourceInfoList.ToArray();

			NUnit.Framework.Assert.That(sourceInfos.Single(c => c.Targets.Contains("Z0_Decimal") && c.ShouldPurge && c.Condition == "Z0_Bool"), Is.Not.EqualTo(default(BasePurgeHelper<DummyBoForAttribute>.PurgeSourceInfo)));
			NUnit.Framework.Assert.That(sourceInfos.Single(c => c.Targets.Contains("Z0_SmallDateTime") && c.ShouldPurge && c.Condition == "NeedPurge"), Is.Not.EqualTo(default(BasePurgeHelper<DummyBoForAttribute>.PurgeSourceInfo)));
			NUnit.Framework.Assert.That(sourceInfos.Single(c => c.Targets.Contains("Z0_Customs001") && c.ShouldPurge && string.IsNullOrEmpty(c.Condition)), Is.Not.EqualTo(default(BasePurgeHelper<DummyBoForAttribute>.PurgeSourceInfo)));
			NUnit.Framework.Assert.That(sourceInfos.Single(c => c.Targets.Contains("Z0_Number") && !c.ShouldPurge && c.Condition == "Z0_Bool"), Is.Not.EqualTo(default(BasePurgeHelper<DummyBoForAttribute>.PurgeSourceInfo)));
		}

		[ExpectNoExceptions]
		public void TestPurgeAllValues_PurgeValueAttribute()
		{
			var obj = Factory.New<DummyBoForPurge>();
			obj.Z0_DateField = ZDate.BrettsBirthday;

			SetupDummyData(obj);

			obj.RelatedCollection = new DummyBusinessObjectCollection(Factory);
			var relatedBo = obj.RelatedCollection.AddNew();

			NUnit.Framework.Assert.That(obj.RelatedCollection, Is.Not.EqualTo(default(DummyBusinessObjectCollection)));
			NUnit.Framework.Assert.That(relatedBo.IsDeleted, Is.EqualTo(false));
			NUnit.Framework.Assert.That(obj.RelatedCollection.Count, Is.EqualTo(1));

			AssertDummyData(obj, ZGuid.BrettsGuid, "AAAA", ZBool.True, 9, 2017, "BBBB", 15.08m, ZDateTime.BrettsBirthday);

			obj.Z0_CalBool = false;

			NUnit.Framework.Assert.That(obj.RelatedCollection, Is.Not.EqualTo(default(DummyBusinessObjectCollection)));
			NUnit.Framework.Assert.That(relatedBo.IsDeleted, Is.EqualTo(false));
			NUnit.Framework.Assert.That(obj.RelatedCollection.Count, Is.EqualTo(1));

			AssertDummyData(obj, ZGuid.BrettsGuid, "AAAA", ZBool.True, 9, 2017, "BBBB", 15.08m, ZDateTime.BrettsBirthday);

			obj.Z0_CalBool = true;

			NUnit.Framework.Assert.That(obj.RelatedCollection, Is.Not.EqualTo(default(DummyBusinessObjectCollection)));
			NUnit.Framework.Assert.That(relatedBo.IsDeleted, Is.EqualTo(true));
			NUnit.Framework.Assert.That(obj.RelatedCollection.Count, Is.EqualTo(0));

			AssertDummyData(obj, ZGuid.BrettsGuid, ZString.Empty, ZBool.False, 0, 0, ZString.Empty, ZDecimal.Zero, ZDateTime.Empty);
		}

		[ExpectNoExceptions]
		public void TestPurgeAllValues_PurgeValueExceptAttribute()
		{
			var obj = Factory.New<DummyBoForPurgeExcept>();
			obj.Z0_DateField = ZDate.BrettsBirthday;

			SetupDummyData(obj);

			obj.RelatedCollection = new DummyBusinessObjectCollection(Factory);
			var relatedBo = obj.RelatedCollection.AddNew();

			NUnit.Framework.Assert.That(obj.RelatedCollection, Is.Not.EqualTo(default(DummyBusinessObjectCollection)));
			NUnit.Framework.Assert.That(relatedBo.IsDeleted, Is.EqualTo(false));
			NUnit.Framework.Assert.That(obj.RelatedCollection.Count, Is.EqualTo(1));

			AssertDummyData(obj, ZGuid.BrettsGuid, "AAAA", ZBool.True, 9, 2017, "BBBB", 15.08m, ZDateTime.BrettsBirthday);

			obj.NeedPurgeExcept = true;

			NUnit.Framework.Assert.That(obj.RelatedCollection, Is.Not.EqualTo(default(DummyBusinessObjectCollection)));
			NUnit.Framework.Assert.That(relatedBo.IsDeleted, Is.EqualTo(false));
			NUnit.Framework.Assert.That(obj.RelatedCollection.Count, Is.EqualTo(1));

			AssertDummyData(obj, ZGuid.BrettsGuid, "AAAA", ZBool.True, 9, 2017, "BBBB", 15.08m, ZDateTime.BrettsBirthday);

			obj.NeedPurgeExcept = false;

			NUnit.Framework.Assert.That(obj.RelatedCollection, Is.Not.EqualTo(default(DummyBusinessObjectCollection)));
			NUnit.Framework.Assert.That(relatedBo.IsDeleted, Is.EqualTo(true));
			NUnit.Framework.Assert.That(obj.RelatedCollection.Count, Is.EqualTo(0));

			AssertDummyData(obj, ZGuid.BrettsGuid, ZString.Empty, ZBool.False, 0, 0, ZString.Empty, ZDecimal.Zero, ZDateTime.Empty);
		}

		[ExpectNoExceptions]
		public void TestHasDataToPurge()
		{
			var obj = Factory.New<DummyBoForPurge>();
			obj.Z0_Description = "";
			obj.Z0_Code = "";

			NUnit.Framework.Assert.That(((IPurgeValueParent)obj).PurgeHelper.HasValueNeededToBePurged(), Is.EqualTo(false), "When every data is empty it shouldn't need to be purged");

			obj.Z0_Description = "TEST";

			NUnit.Framework.Assert.That(((IPurgeValueParent)obj).PurgeHelper.HasValueNeededToBePurged(), Is.EqualTo(true), "Description should need to be purged");

			obj.Z0_Description = "";

			obj.RelatedCollection = new DummyBusinessObjectCollection(Factory);
			obj.RelatedCollection.AddNew();

			NUnit.Framework.Assert.That(((IPurgeValueParent)obj).PurgeHelper.HasValueNeededToBePurged(), Is.EqualTo(true), "A collection needs to be purged");
		}

		#region Dummy Bo

		sealed class DummyBoForAttribute : DummyBoForPurge
		{
			public DummyBoForAttribute(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[PurgeValue("Z0_Bool")]
			public override ZDecimal Z0_Decimal
			{
				get => base.Z0_Decimal;
				set => base.Z0_Decimal = value;
			}

			[PurgeValueExcept("Z0_Bool")]
			public override ZInt Z0_Number
			{
				get => base.Z0_Number;
				set => base.Z0_Number = value;
			}
		}

		class DummyBoForPurge : DummyBusinessObject, IPurgeValueParent
		{
			public DummyBoForPurge(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				calBool = false;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Member called through reflection")]
			bool NeedPurge => Z0_CalBool;

			public ZBool Z0_CalBool
			{
				get => calBool;
				set
				{
					calBool = value;

					if (calBool)
					{
						PurgeHelper.PurgeAllValues();
					}
				}
			}
			ZBool calBool;

			[PurgeValue]
			public virtual ZString Z0_Customs001
			{
				get => z0_Customs001;
				set => z0_Customs001 = value;
			}
			ZString z0_Customs001;

			[PurgeValue("NeedPurge")]
			public override ZString Z0_Description
			{
				get => base.Z0_Description;
				set => base.Z0_Description = value;
			}

			[PurgeValue("NeedPurge")]
			public override ZString Z0_Code
			{
				get => base.Z0_Code;
				set => base.Z0_Code = value;
			}

			[PurgeValue("NeedPurge")]
			public override ZBool Z0_Bool
			{
				get => base.Z0_Bool;
				set => base.Z0_Bool = value;
			}

			[PurgeValue("NeedPurge")]
			public override ZShort Z0_Short
			{
				get => base.Z0_Short;
				set => base.Z0_Short = value;
			}

			[PurgeValue("NeedPurge")]
			public override ZInt Z0_Number
			{
				get => base.Z0_Number;
				set => base.Z0_Number = value;
			}

			[PurgeValue("NeedPurge")]
			public override ZDecimal Z0_Money
			{
				get => base.Z0_Money;
				set => base.Z0_Money = value;
			}

			[PurgeValue("NeedPurge")]
			public override ZDateTime Z0_SmallDateTime
			{
				get { return base.Z0_SmallDateTime; }
				set { base.Z0_SmallDateTime = value; }
			}

			[PurgeValue("NeedPurge")]
			public ZDateTime Z0_DateField
			{
				get; set;
			}

			[PurgeValue("NeedPurge")]
			public DummyBusinessObjectCollection RelatedCollection
			{
				get; set;
			}

			#region IPurgeWitMacroParent

			public bool IsPurging
			{
				get; set;
			}

			public IPurgeValueHelper PurgeHelper
			{
				get => _purgeValueHelper ?? (_purgeValueHelper = new PurgeValueHelper<DummyBoForPurge>(this));
			}
			PurgeValueHelper<DummyBoForPurge> _purgeValueHelper;

			#endregion
		}

		class DummyBoForPurgeExcept : DummyBusinessObject, IPurgeValueParent
		{
			public DummyBoForPurgeExcept(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				needPurgeExcept = true;
			}

			public bool NeedPurgeExcept
			{
				get => needPurgeExcept;
				set
				{
					needPurgeExcept = value;
					PurgeHelper.PurgeAllValues();
				}
			}
			bool needPurgeExcept;

			[PurgeValueExcept]
			public virtual ZString Z0_Customs001
			{
				get => z0_Customs001;
				set => z0_Customs001 = value;
			}
			ZString z0_Customs001;

			[PurgeValueExcept("NeedPurgeExcept")]
			public override ZString Z0_Description
			{
				get => base.Z0_Description;
				set => base.Z0_Description = value;
			}

			[PurgeValueExcept("NeedPurgeExcept")]
			public override ZString Z0_Code
			{
				get => base.Z0_Code;
				set => base.Z0_Code = value;
			}

			[PurgeValueExcept("NeedPurgeExcept")]
			public override ZBool Z0_Bool
			{
				get => base.Z0_Bool;
				set => base.Z0_Bool = value;
			}

			[PurgeValueExcept("NeedPurgeExcept")]
			public override ZShort Z0_Short
			{
				get => base.Z0_Short;
				set => base.Z0_Short = value;
			}

			[PurgeValueExcept("NeedPurgeExcept")]
			public override ZInt Z0_Number
			{
				get => base.Z0_Number;
				set => base.Z0_Number = value;
			}

			[PurgeValueExcept("NeedPurgeExcept")]
			public override ZDecimal Z0_Money
			{
				get => base.Z0_Money;
				set => base.Z0_Money = value;
			}

			[PurgeValueExcept("NeedPurgeExcept")]
			public override ZDateTime Z0_SmallDateTime
			{
				get { return base.Z0_SmallDateTime; }
				set { base.Z0_SmallDateTime = value; }
			}

			[PurgeValueExcept("NeedPurgeExcept")]
			public ZDateTime Z0_DateField
			{
				get; set;
			}

			[PurgeValueExcept("NeedPurgeExcept")]
			public DummyBusinessObjectCollection RelatedCollection
			{
				get; set;
			}

			#region IPurgeWitMacroParent

			public bool IsPurging
			{
				get; set;
			}

			public IPurgeValueHelper PurgeHelper
			{
				get => _purgeValueHelper ?? (_purgeValueHelper = new PurgeValueHelper<DummyBoForPurgeExcept>(this));
			}
			PurgeValueHelper<DummyBoForPurgeExcept> _purgeValueHelper;

			#endregion
		}

		#endregion
	}
}
