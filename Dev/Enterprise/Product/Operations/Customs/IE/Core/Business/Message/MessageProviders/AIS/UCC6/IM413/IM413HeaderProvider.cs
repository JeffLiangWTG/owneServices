using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IE.Business.AIS
{
	[CodeAlive("Part of a message builder in development")]
	public class IM413HeaderProvider : IM413_414_415_432_433HeaderProvider, IIM413Header
	{
		public IM413HeaderProvider(AISMessageSendingAction sendingAction) : base(sendingAction) { }

		public IIM413Operation ImportOperation => CachedValueHelper.GetValue(ref importOperation, () => new IM413OperationProvider(entryHeader));
		CachedValue<IM413OperationProvider> importOperation;
	}
}
