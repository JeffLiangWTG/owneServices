using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE829;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE829Provider : IIE829
	{
		public IE829Provider(Ie829Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie829Type message;

		public ZString SendingCustomsOffice => message.Body.NotificationOfAcceptedExport.ExportDeclarationAcceptanceRelease.ReferenceNumberOfSenderCustomsOffice;

		public ZDate AcceptanceDate => new ZDate(message.Body.NotificationOfAcceptedExport.ExportDeclarationAcceptanceRelease.DateOfAcceptance);

		public ZDate ReleaseDate => new ZDate(message.Body.NotificationOfAcceptedExport.ExportDeclarationAcceptanceRelease.DateOfRelease);

		public IReadOnlyCollection<IEMCSEvent> ExciseMovementEads => exciseMovementEads ?? (exciseMovementEads = message.Body.NotificationOfAcceptedExport.ExciseMovementEad.Select(x => new IE829EventProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSEvent> exciseMovementEads;

		public ZString MrnNumber => ExciseMovementEad?.AdministrativeReferenceCode ?? ZString.Empty;

		public ZString MrnNumberSequenceNumber => ExciseMovementEad?.SequenceNumber ?? ZString.Empty;

		public ZString Mrn => message.Body.NotificationOfAcceptedExport.ExportDeclarationAcceptanceRelease.DocumentReferenceNumber;

		IEMCSEvent ExciseMovementEad
		{
			get
			{
				if (exciseMovementEad == null)
				{
					var exciseMovementEads = ExciseMovementEads.Take(2).ToArray();
					if (exciseMovementEads.Length == 1)
					{
						exciseMovementEad = exciseMovementEads[0];
					}
				}

				return exciseMovementEad;
			}
		}

		IEMCSEvent exciseMovementEad;
	}

	class IE829EventProvider : IEMCSEvent
	{
		public IE829EventProvider(ExciseMovementEadType exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ExciseMovementEadType exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;

		public ZBool ExportDeclarationAcceptanceOrGoodsReleasedForExport => CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.Flag.Item1.Equals(exciseMovementEad.ExportDeclarationAcceptanceOrGoodsReleasedForExport) ? ZBool.True : ZBool.False;
	}
}
