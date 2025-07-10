using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.Statement;

public class DocStatementChargesCollection : DocBaseWrapperCollection<DocStatementCharge>
{
	public DocStatementChargesCollection(CusStatementLineChargeCollection cusStatementLineChargeCollection, BusinessObjectFactory factory)
		: base(factory)
	{
		foreach (var cusStatementLineCharge in cusStatementLineChargeCollection.Cast<CusStatementLineCharge>())
		{
			Add(DocStatementCharge.New(cusStatementLineCharge, factory));
		}
	}
}
