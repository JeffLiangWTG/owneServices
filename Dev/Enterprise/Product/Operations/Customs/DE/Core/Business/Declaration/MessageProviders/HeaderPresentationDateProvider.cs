using System;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used In HeaderPresentationDateProviderTest")]
	public class HeaderPresentationDateProvider : IDateTimeRange
	{
		public static HeaderPresentationDateProvider NewOrNull(JobDeclaration declaration) => declaration == null ? null : new HeaderPresentationDateProvider(declaration);

		HeaderPresentationDateProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public DateTime StartDateTime => declaration.ZG_PresentationStartDate.ZeroFromSecond().ToDateTime();

		public DateTime EndDateTime => declaration.ZG_PresentationEndDate.ZeroFromSecond().ToDateTime();
	}
}
