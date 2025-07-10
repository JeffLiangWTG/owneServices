using System.CodeDom;
using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI.Design
{
	class KSplitContainerSerializer : ControlDpiScalingCodeDomSerializer
	{
		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			var statements = (CodeStatementCollection)base.Serialize(manager, value);

			if (statements != null)
			{
				FixKSplitContainer(statements);
			}

			return statements;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		void FixKSplitContainer(CodeStatementCollection statements)
		{
			int preferredLoc = -1;
			for (int i = 0; i < statements.Count; ++i)
			{
				var stmtName = ((statements[i] as CodeAssignStatement)?.Left as CodePropertyReferenceExpression)?.PropertyName;
				if ((stmtName == "Panel1MinSize" || stmtName == "Panel2MinSize") && preferredLoc < 0)
				{
					preferredLoc = i;
				}
				else if (stmtName == "Size")
				{
					if (preferredLoc >= 0)
					{
						var size = statements[i];
						statements.RemoveAt(i);
						statements.Insert(preferredLoc, size);
					}
					return;
				}
			}
		}
	}
}
