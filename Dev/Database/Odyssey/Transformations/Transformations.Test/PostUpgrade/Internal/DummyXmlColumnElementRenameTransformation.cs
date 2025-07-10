using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	class DummyXmlColumnElementRenameTransformation : XmlColumnElementRenameTransformation
	{
		internal IEnumerable<ElementRenameDetails> ElementsToRenameOverride { get; set; }

		protected internal override SchemaColumn XmlColumn
		{
			get { return BMBoardSectionSchema.MS_LayoutData; }
		}

		protected internal override string SetAuditColumnsExpression
		{
			get => @",MS_SystemLastEditTimeUTC = getutcdate(), MS_SystemLastEditUser = '~BP'";
		}

		protected override IEnumerable<ElementRenameDetails> ElementsToRename
		{
			get { return ElementsToRenameOverride; }
		}

		public override string UserDescription
		{
			get { return string.Empty; }
		}
	}
}
