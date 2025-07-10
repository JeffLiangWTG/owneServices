using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.IE.NCTS.Business.Constants;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsHeaderDocumentSupporter : EU.NCTS.Business.NctsHeaderDocumentSupporter
	{
		public NctsHeaderDocumentSupporter(NctsHeader parent) : base(parent)
		{
		}

		#region Implementation

		NctsHeader NctsHeader => (NctsHeader)BusinessObject;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			BusinessObject businessObjectToWrap = null;
			switch (dataContext)
			{
				case DataContext.EuNcts:
					businessObjectToWrap = NctsHeader;
					break;
			}

			if (businessObjectToWrap == null)
			{
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}

			return CreateDocumentWrappers(dataContext, businessObjectToWrap);
		}

		DocumentWrapper[] CreateDocumentWrappers(DataContext dataContext, BusinessObject businessObjectToWrap)
		{
			var wrapperStrongName = supportedWrappers[dataContext];
			return new[] { DocumentWrapperFactory.CreateWrapperWithoutException(wrapperStrongName, businessObjectToWrap, "Enterprise.Customs.IE.NCTS.DocumentWrappers") }
					.WhereNotNull()
					.ToArray();
		}

		readonly ImmutableDictionary<DataContext, ZString> supportedWrappers = new Dictionary<DataContext, ZString>()
		{
			{ DataContext.EuNcts, DocumentWrapperConstants.FullNames.NctsHeaderAccompanyingDocument },
		}.ToImmutableDictionary();

		#endregion
	}
}
