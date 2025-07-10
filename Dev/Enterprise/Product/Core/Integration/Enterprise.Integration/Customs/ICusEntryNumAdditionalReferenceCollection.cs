using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryNumAdditionalReferenceCollection : IBusinessObjectCollection
		{
			new ICusEntryNumber this[int i] { get; }
			new ICusEntryNumber AddNew();
			ICusEntryNumber AddNewIfNotExist(ZString type, ZString number);
			ICusEntryNumber GetFirstReferenceNumberByType(ZString type);
			ZString[] GetAllReferenceNumbersByType(ZString type);
			void RemoveAndDelete(ICusEntryNumber entryNum);
		}
	}
}
