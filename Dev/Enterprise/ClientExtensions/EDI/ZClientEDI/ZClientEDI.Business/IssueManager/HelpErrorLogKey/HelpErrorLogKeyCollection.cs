
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class HelpErrorLogKeyCollection : DependentBusinessObjectCollection<HelpErrorLogKey, EdiHelpErrorLog>
	{
		public HelpErrorLogKeyCollection(EdiHelpErrorLog master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public bool ContainsKey(string key, int hashCode)
		{
			foreach (HelpErrorLogKey errorLogKey in this)
			{
				if ((hashCode == errorLogKey.HK_HashCode || hashCode == 0 || errorLogKey.HK_HashCode == 0) && key == errorLogKey.HK_Key)
				{
					return true;
				}
			}

			return false;
		}
	}
}

