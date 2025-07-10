using System.Data;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	[CodeProperty(DummyBizoSchema.Constants.Z0_Code), DescriptionProperty(DummyBizoSchema.Constants.Z0_Description)]
	public class DummyBusinessObjectWithMultilingualStringProperty : DummyBusinessObject
	{
		public DummyBusinessObjectWithMultilingualStringProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[TranslatableDataField(Schema.TableName, Schema.Z0_Description, MaxLength = Schema.Z0_DescriptionMaxLength, Type = typeof(TranslatableDataFieldTestCase.DummyWithTranslatable), Asmid = ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId, SecurityCheckpoint = "System")]
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
