using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class VesselProvider : IVessel
	{
		public VesselProvider(JobDeclaration declaration, MessageSendingObject sendingObject)
		{
			Argument.NotNull(declaration, nameof(declaration));
			this.declaration = declaration;

			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly JobDeclaration declaration;

		readonly MessageSendingObject sendingObject;

		public string Code => declaration.JE_RadioCallSign;

		public string Name
		{
			get
			{
				if (declaration.IsAir && sendingObject.ProcedureCode == JPProcedureCodeList.Codes.IDA)
				{
					var result = new ZStringBuilder();
					result.AppendIfNotEmpty(declaration.JE_VoyageFlightNo);
					result.AppendIfNotEmpty(declaration.JE_ExportDate.ToString("ddMMM", CultureInfo.InvariantCulture).ToUpper());
					return result.ToStringWithDelimiterBetweenAppends("/");
				}
				else if (declaration.IsBasketRadioCallSign)
				{
					return declaration.JE_VesselName;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}
	}
}
