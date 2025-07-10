using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class CUSTAXProvider : ICUSTAX
	{
		public CUSTAXProvider(GCTAXM message)
		{
			this.message = CargoWise.Common.Argument.NotNull(message, nameof(message));
		}

		public ZString ReferenceNumber => message.Header?.ReferenceNumber ?? ZString.Empty;

		public string MRN => message.Header?.MRN;

		public ZString CompletionFlag => message.Header?.CompletionFlag ?? ZString.Empty;

		public ZString LocalReferenceNumber => message.Header?.LRN ?? ZString.Empty;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public DateTime? RegistrationDate => (message.Header?.RegistrationDateSpecified ?? false) ? message.Header.RegistrationDate : null;

		public IReadOnlyCollection<ICUSTAXLine> Lines => lines ?? (lines = message.Body.GoodsItem.Select(x => new CUSTAXLineProvider(x)).ToArray());

		public ZDecimal TotalCustomsDutyAmount => message.CustomsDuties?.TotalCustomsDuty.Amount ?? ZDecimal.Zero;

		public DateTime? AcceptanceDate
		{
			get
			{
				var acceptanceDates = message.Body.GoodsItem?.Where(x => x.CustomsTaxAssessment != null && x.CustomsTaxAssessment.AcceptanceDateSpecified)?.Select(x => x.CustomsTaxAssessment.AcceptanceDate);
				return !acceptanceDates.IsNullOrEmpty() ? acceptanceDates.Min() : null;
			}
		}

		IReadOnlyCollection<ICUSTAXLine> lines;

		readonly GCTAXM message;
	}
}
