using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class NctsCustomsDocDataObjectProvider : EU.NCTS.Business.Documents.DocDataObjects.NctsCustomsDocDataObjectProvider
	{
		protected override object GetDocDataObjectCore(object nctsHeader, string dataContext, IDocDataObjectParameters parameters)
		{
			if (nctsHeader is NctsHeader header)
			{
				switch (dataContext)
				{
					case DataContext.FRPortsRegularizationTransitDOA:
						return new DOABuilder(header).Build();
					case DataContext.FRPortsCustomsCheckCAED:
						return new NctsCAEDBuilder(header).Build();
				}
			}

			return base.GetDocDataObjectCore(nctsHeader, dataContext, parameters);
		}
	}
}
