using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonCompleteTransitOperationWrapper : NCTS5CommonTransitOperationWrapper, INCTSCommonCompleteTransitOperation
	{
		public NCTS5CommonCompleteTransitOperationWrapper(NctsHeader header, ZString messageType) : base(header)
		{
			this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
		}
		readonly ZString messageType;

		const string AdditionalDeclarationCodeDeparture = "A";
		const string AdditionalDeclarationCodePreDeclarationAndAmendment = "D";

		public ZString AdditionalDeclarationType => AdditionalDeclarationCodePreDeclarationAndAmendmentCodesList(messageType) ? AdditionalDeclarationCodePreDeclarationAndAmendment : AdditionalDeclarationCodeDeparture;

		public ZBool ReducedDatasetIndicator => departureMovement.BM_ReducedDatasetIndicator;

		public ZString SpecificCircumstanceIndicator => departureMovement.BM_SpecificCircumstance;

		static ZBool AdditionalDeclarationCodePreDeclarationAndAmendmentCodesList(ZString code)
		{
			var codesList = new ZString[]
			{
				DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration,
				DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment,
			};
			return codesList.Contains(code);
		}
	}
}
