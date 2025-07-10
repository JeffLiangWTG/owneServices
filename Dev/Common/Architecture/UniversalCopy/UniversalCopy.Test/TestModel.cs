using System;
using System.Collections.Generic;
using System.ComponentModel;
using WTG.Glow.Data.Annotations;
using DescriptionAttribute = WTG.Glow.Data.Annotations.DescriptionAttribute;

namespace CargoWise.UniversalCopy.Test
{
	[CollectionRelationProperty("SimpleElements", "E1_TM", "SimpleElements")]
	[CollectionRelationProperty("RecursiveElements", "E2_Parent", "RecursiveElements")]
	[TableNameProvider("TestModel")]
	public interface ITestModel : IBitmapHolder
	{
		[PrimaryKey]
		Guid TM_PK { get; }

		[Code]
		[MaxLength(3)]
		string TM_Code { get; set; }

		[Description]
		[MaxLength(20)]
		string TM_Description { get; set; }

		[DefaultValue(5)]
		int TM_Number { get; set; }

		[DefaultValue(true)]
		bool TM_Bool { get; set; }

		[Scale(4)]
		[Precision(19)]
		decimal TM_Decimal { get; set; }

		string TM_Name { get; }

		Guid TM_E3_A { get; set; }

		Guid TM_E3_B { get; set; }

		[RelationProperty(nameof(TM_E3_A))]
		RelatedElement RelatedElementA { get; set; }

		[RelationProperty(nameof(TM_E3_B))]
		RelatedElement RelatedElementB { get; }

		ICollection<SimpleElement> SimpleElements { get; }

		ICollection<RecursiveElement> RecursiveElements { get; }
	}

	[ResourceStream("TM_Bitmap", "bitmap", true)]
	public interface IBitmapHolder { }

	[TableNameProvider("RelatedElement")]
	public interface RelatedElement
	{
		[PrimaryKey]
		Guid E3_PK { get; }

		[Code]
		[Description]
		[MaxLength(3)]
		string E3_Code { get; set; }

		Guid E3_E1 { get; set; }

		Guid E3_E3 { get; set; }

		[RelationProperty(nameof(E3_E1))]
		SimpleElement SimpleElement { get; }

		[RelationProperty(nameof(E3_E3))]
		RelatedElement SubRelatedElement { get; }
	}

	[TableNameProvider("SimpleElement")]
	[ParentTableColumn("E1_ParentTable")]
	public interface SimpleElement
	{
		[PrimaryKey]
		Guid E1_PK { get; }

		[Code]
		[Description]
		[MaxLength(3)]
		string E1_Code { get; set; }

		Guid E1_TM { get; set; }
	}

	[CollectionRelationProperty("Children", "E2_Parent", "Children")]
	[TableNameProvider("RecursiveElement")]
	public interface RecursiveElement
	{
		[PrimaryKey]
		Guid E2_PK { get; }

		[Code]
		[Description]
		[MaxLength(3)]
		string E2_Code { get; set; }

		Guid E2_Parent { get; set; }

		ICollection<RecursiveElement> Children { get; }
	}
}
