using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocMatchLinkCollection : DocumentWrapperCollection
	{
		public DocMatchLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocMatchLink this[int index]
		{
			get { return (DocMatchLink)base[index]; }
		}

		public ZGuid[] PKs
		{
			get
			{
				return this.Select(docMatchLink => docMatchLink.PK).Distinct().ToArray();
			}
		}

		public ZString[] GroupNums
		{
			get
			{
				return this.Select(docMatchLink => ((DocMatchLink)docMatchLink).MatchGroupNum).Distinct().ToArray();
			}
		}

		public ZDecimal TotalAmount
		{
			get
			{
				return this.Sum(docMatchLink => ((DocMatchLink)docMatchLink).Amount);
			}
		}

		public ZDecimal TotalMatchAmount
		{
			get
			{
				return this.Sum(docMatchLink => ((DocMatchLink)docMatchLink).MatchAmount);
			}
		}

		public ZDecimal TotalGSTRealised
		{
			get
			{
				return this.Sum(docMatchLink => ((DocMatchLink)docMatchLink).GSTRealised);
			}
		}
	}
}
