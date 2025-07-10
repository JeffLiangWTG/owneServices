using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXDVDSendMessageWrapper : DVDCommonSendMessageWrapper, IComplXDVDMessageDataProvider
	{
		public ComplXDVDSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
		}

		const string MRNPrefix = "DVD";

		protected override ZString MRNCore => MRNPrefix + entryHeader.MovementReferenceNumber;

		public ZInt TotalPackages => entryHeader.PackagesCount;

		public ZDecimal TotalGrossMass => (ZDecimal)entryHeader.MergedLines.Sum(x => x.EffectiveGrossWeight.InUnroundedKilogramsSafe);

		public IComplXDVDDeclarantAndRepresentative DeclarantAndRepresentative => declarantAndRepresentative ?? (declarantAndRepresentative = new ComplXDVDDeclarantAndRepresentativeWrapper(declaration));
		ComplXDVDDeclarantAndRepresentativeWrapper declarantAndRepresentative;

		public IReadOnlyCollection<IComplXDVDLine> Lines => lines ?? (lines = entryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new ComplXDVDLineWrapper(x)).ToList().AsReadOnly());
		IReadOnlyCollection<ComplXDVDLineWrapper> lines;
	}
}
