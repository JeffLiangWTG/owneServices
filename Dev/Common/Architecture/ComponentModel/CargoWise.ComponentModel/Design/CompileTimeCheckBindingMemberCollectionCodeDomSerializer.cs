using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Security;

namespace CargoWise.ComponentModel.Design
{
	[SecurityCritical]
	public class CompileTimeCheckBindingMemberCollectionCodeDomSerializer : KCodeDomSerializer
	{
		public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
		{
			return new CompileTimeCheckBindingMemberCollection();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Code comment string")]
		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			var result = new CodeStatementCollection();
			if (manager != null && value != null)
			{
				var serialiser = (CodeDomSerializer)manager.GetSerializer(typeof(CompileTimeCheckBindingMember), typeof(CodeDomSerializer));
				if (serialiser != null)
				{
					foreach (var current in (IList)value)
					{
						if (current != null && current.GetType().FullName != typeof(CompileTimeCheckBindingMember).FullName && current.GetType().BaseType != null && current.GetType().BaseType.FullName != typeof(CompileTimeCheckBindingMember).FullName)
						{
							throw new InvalidCastException();
						}

						var statement = (CodeStatement)serialiser.Serialize(manager, current);
						if (statement != null)
						{
							result.Add(statement);
						}
					}
				}
				if (result.Count > 0)
				{
					result.Insert(0, new CodeCommentStatement("The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again."));
				}
			}
			return result;
		}
	}
}
