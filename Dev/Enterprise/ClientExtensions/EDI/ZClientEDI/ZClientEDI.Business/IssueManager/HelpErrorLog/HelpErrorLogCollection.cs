using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class HelpErrorLogCollection : BusinessObjectCollection<EdiHelpErrorLog>, IHelpErrorLogCollection
	{
		public HelpErrorLogCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetReadOnlyIncludingChildren(true);
		}

		public override string ToString()
		{
			return Count.ToString(CultureInfo.InvariantCulture) + " Log" + (Count == 1 ? "" : "s");
		}

		#region IHelpErrorLogCollection Members

		public EdiHelpErrorLog Find(ZString key)
		{
			ZInt hash = HelpErrorLogKey.GetKeyHashCode(key);

			foreach (EdiHelpErrorLog log in this)
			{
				if (log.Keys.ContainsKey(key, hash))
				{
					return log;
				}
			}

			return null;
		}

		public void Save()
		{
			Factory.Save();
		}

		public EdiHelpErrorLog AddNew(ZString key)
		{
			return AddNew();
		}

		#endregion
	}
}

