using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using CargoWise.Common;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// A collection of CompileTimeCheckBindingMember objects that can be exposed on a designable component to
	/// provide compile time checking for multiple members.
	/// </summary>
	[Serializable]
	[DesignerSerializer(typeof(CompileTimeCheckBindingMemberCollectionCodeDomSerializer), typeof(CodeDomSerializer))]
	public class CompileTimeCheckBindingMemberCollection : Collection<CompileTimeCheckBindingMember>
	{
		public void AddRange(IEnumerable<CompileTimeCheckBindingMember> values)
		{
			Argument.NotNull(values, nameof(values)); // Suggested By ReviewBot 
			foreach (var item in values)
			{
				Add(item);
			}
		}
	}
}
