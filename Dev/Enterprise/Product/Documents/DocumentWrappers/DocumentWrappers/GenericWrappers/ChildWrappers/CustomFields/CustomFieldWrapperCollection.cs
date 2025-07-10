using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[CustomIndexerList(typeof(CustomFieldsList))]
	public class CustomFieldWrapperCollection : GenericWrapperCollection<CustomFieldWrapper>
	{
		public CustomFieldWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IBODocDataProvider GetRow(ZString index)
		{
			var customField = this.Cast<CustomFieldWrapper>().FirstOrDefault(c => c.CustomFieldName == index);
			return customField ?? base.GetRow(index);
		}
	}

	public class CustomFieldsList : CodeDescriptionPairList
	{
		public CustomFieldsList()
		{
			foreach (ICodeDescription codeDescriptionPair in new CodeDescriptionPairList(OLookUpEditType.CustomLabels))
			{
				Add(codeDescriptionPair);
			}
		}
	}
}
