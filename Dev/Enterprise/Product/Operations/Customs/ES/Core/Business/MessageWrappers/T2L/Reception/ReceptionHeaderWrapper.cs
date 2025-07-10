using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ReceptionHeaderWrapper : IReceptionHeader
	{
		public ReceptionHeaderWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			jobDeclaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration jobDeclaration;

		public ZString ReceptionCustomsOffice => jobDeclaration.JE_CustomsOffice.Right(6);

		public ZString ReceptionT2LReference => entryHeader.MovementReferenceNumber;

		public ZDateTime ExpeditionDate => entryHeader.MovementReferenceNumberIssueDate;

		public ZString ExpeditionCountry => jobDeclaration.JE_GoodsOrigin;

		public ZInt TotalLinesNum => entryHeader.MergedLines.Count;

		public ZInt TotalPackagesQty => entryHeader.PackagesCount;

		public ZBool ContainersIndicator => entryHeader.IsContainerised();

		public IPartyNameProvider Declarant => CachedValueHelper.GetValue(ref declarant, () => PartyNameWrapper.New(jobDeclaration.Declarant));
		CachedValue<IPartyNameProvider> declarant;
	}
}
