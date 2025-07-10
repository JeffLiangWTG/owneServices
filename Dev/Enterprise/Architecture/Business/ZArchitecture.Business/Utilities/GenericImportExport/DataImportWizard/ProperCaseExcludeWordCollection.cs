using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using ResString = Enterprise.ZArchitecture.Business.ResString;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ProperCaseExcludeWordCollection : NonPersistentBusinessObjectCollection<ProperCaseExcludeWord>
	{
		public ProperCaseExcludeWordCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ProperCaseExcludeWordCollection(BusinessObjectFactory factory, IEnumerable<string> words)
			: base(factory)
		{
			using (SuspendListChanged())
			{
				foreach (var word in words)
				{
					AddNew().Word = word;
				}
			}
		}

		public void ReplaceWords(IEnumerable wordCollection)
		{
			RemoveAll();
			AddRange(wordCollection);
		}

		public ProperCaseExcludeWordCollection GetCopy()
		{
			var words = this.Select(excludeWord => excludeWord.Word.ToString());
			return new ProperCaseExcludeWordCollection(Factory, words);
		}

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return ResString.GetMultilingualString("ec62baad-f8a4-4f5f-9707-9af960d3ff8f", "Proper Case Exclude List"); }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProperCaseExcludeWord(Factory);
		}

		#endregion
	}
}
