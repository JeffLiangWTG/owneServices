using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class LocalPartNumberCollection : BaseCusGoodsCatalogProductionInfoCollection<LocalPartNumber>
	{
		public LocalPartNumberCollection(CusGoodsCatalog parent) : base(parent, CusGoodsCatalogProductionInfoTypeList.Codes.LPN)
		{
			catalog = parent;
		}
		readonly CusGoodsCatalog catalog;

		protected override bool AllowNew => false;

		public IEnumerable<LocalPartNumber> Find(string partNum) => this.Cast<LocalPartNumber>().Where(x => x.CGI_Reference == partNum);

		public void AddLocalPartNumberIfNotExists(string partNum)
		{
			if (!Find(partNum).Any())
			{
				AddNew().CGI_Reference = partNum;
			}
		}

		internal void RemoveLocalPartNumberIfExists(string partNum)
		{
			Find(partNum).DeleteAll();
		}

		public void UpdateLocalPartNumber(ZString oldPartNum, ZString newPartNum)
		{
			if (oldPartNum != newPartNum)
			{
				if (!oldPartNum.IsEmpty && !newPartNum.IsEmpty)
				{
					Find(oldPartNum).ForEach(x =>
					{
						x.CGI_Reference = newPartNum;

						if (!catalog.CGC_AuthorityIdentifier.IsEmpty)
						{
							catalog.Logs.AddNew(AutoEvents.ChangeOfIdentifier,
								new KeyValuePair<string, string>(
									CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description,
									$"A new Local Part Number was added because the Product Code was changed from {oldPartNum} to {newPartNum}."));
						}
					});
				}
				else if (oldPartNum.IsEmpty)
				{
					AddLocalPartNumberIfNotExists(newPartNum);
				}
				else if (newPartNum.IsEmpty)
				{
					RemoveLocalPartNumberIfExists(oldPartNum);
				}
			}
		}
	}
}
