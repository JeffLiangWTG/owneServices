using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public class CHEDIMessageCollection : EDIMessageCollection
{
	public CHEDIMessageCollection(BusinessObject master) : base(master)
	{
	}

	public new CHEDIMessage this[int index] => (CHEDIMessage)base[index];

	public new CHEDIMessage AddNew() => (CHEDIMessage)base.AddNew();

	protected override ZQuery CreateAdditionalFilter()
	{
		var result = base.CreateAdditionalFilter();
		result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, new[] { EDIInterchange.ApplicationCodes.CHCustomsEdec, EDIInterchange.ApplicationCodes.CHCustomsPassar, EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput });
		return result;
	}
}
