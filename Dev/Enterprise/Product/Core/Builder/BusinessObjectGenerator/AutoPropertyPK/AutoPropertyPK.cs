using System.Data;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoPropertyPK : AutoProperty
	{
		public AutoPropertyPK(BusinessObjectInfo info, DataColumn column)
			: base(info, column)
		{
		}

		public string PrimaryKeyName
		{
			get { return "PK"; }
		}

		public override string Code
		{
			get
			{
				return LinesOfCode(
					"		public override SchemaGuidColumn PKSchemaColumn",
					"		{",
					"			get { return " + Info.ClassNames.Schema + ".PK; }",
					"		}"
					);
			}
		}

		public override bool GenerateValidation
		{
			get { return false; }
		}
	}
}