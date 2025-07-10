using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GuaranteeWrapper : IGuarantee
	{
		GuaranteeWrapper(GuaranteeForEntryInstruction guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}
		readonly GuaranteeForEntryInstruction guarantee;

		public ICollection<IGuaranteeReference> GuaranteeReference => guaranteeReference ?? (guaranteeReference = GetGuaranteeReferenceCollection());
		ICollection<IGuaranteeReference> guaranteeReference;

		ICollection<IGuaranteeReference> GetGuaranteeReferenceCollection()
		{
			Collection<IGuaranteeReference> result = null;

			if (guarantee != null)
			{
				result = new Collection<IGuaranteeReference>() { GuaranteeReferenceWrapper.New(guarantee) };
			}

			return result;
		}

		public string GuaranteeType => guaranteeType ?? (guaranteeType = guarantee.PW_BondType);
		string guaranteeType;

		public static GuaranteeWrapper New(GuaranteeForEntryInstruction guarantee) => guarantee == null ? null : new GuaranteeWrapper(guarantee);
	}
}
