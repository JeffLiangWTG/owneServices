using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class CustomNoteNameTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public CustomNoteNameTranslatableDataFieldAttribute(string columnName)
			: base(string.Empty, columnName)
		{ }

		protected override string TableDescriptionPrefix
		{
			get { return string.Empty; }
		}

		protected override IEnumerable<ResourceString> GetRuntimeCaptionsFromDatabase(IResString userCaption = null, object context = null)
		{
			foreach (PredefinedNoteType noteType in SystemDataRegistry.Instance.CustomNotes.Value.AllCustomNoteTypes)
			{
				yield return (ResourceString)noteType.MultilingualDescription;
			}
		}

#if DEBUG
		public override BusinessObject[] GetSystemDefinedParentObjectsForTest(BusinessObjectFactory factory, System.Type type)
		{
			return System.Array.Empty<BusinessObject>();
		}
#endif
	}
}
