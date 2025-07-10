using System.Data;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	public sealed class TranslatableDataFieldTestCase : TestCase
	{
		public void TestGetAttributeForColumn()
		{
			AssertNotNull(TranslatableDataFieldAttribute.GetAttributeForColumn("RN_Desc"));
			AssertNotNull(TranslatableDataFieldAttribute.GetAttributeForColumn("RN_Desc", "RefCountry"));
		}

		public class DummyWithTranslatable : DummyBusinessObject
		{
			public DummyWithTranslatable(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[TranslatableDataField(Schema.TableName, Schema.Z0_Description, MaxLength = Schema.Z0_DescriptionMaxLength, Type = typeof(DummyWithTranslatable), Asmid = ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId, SecurityCheckpoint = "System")]
			public override ZString Z0_Description
			{
				get { return base.Z0_Description; }
				set { base.Z0_Description = value; }
			}

			public MultilingualString Z0_DescriptionMultilingual
			{
				get { return GetMultilingual(Z0_DescriptionInfo); }
			}
		}
	}
}
