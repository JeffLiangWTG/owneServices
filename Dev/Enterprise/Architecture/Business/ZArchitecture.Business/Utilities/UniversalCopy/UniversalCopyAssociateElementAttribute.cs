using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	/// <summary>
	/// Provides association between element definition in GLOW and actual element on component when their names are different.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class UniversalCopyAssociateElementAttribute : Attribute
	{
		/// <summary>
		/// Provides association between element definition in GLOW and actual element on component when their names are different.
		/// </summary>
		/// <param name="definitionElement">Name of element in GLOW model definition.</param>
		/// <param name="componentElement">Name of property on component.</param>
		public UniversalCopyAssociateElementAttribute(string definitionElement, string componentElement)
		{
			DefinitionElement = definitionElement;
			ComponentElement = componentElement;
		}

		public string DefinitionElement { get; private set; }
		public string ComponentElement { get; private set; }
	}
}
