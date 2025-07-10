using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class TransportMeansValidation : CusCodeDataValidation
	{
		public TransportMeansValidation(TransportMeans parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDescription();
		}

		protected override void CheckCY_Code()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo);

			if (Parent.CY_Code == ZString.Empty && Parent.CY_Data == ZString.Empty)
			{
				Parent.CY_CodeInfo.AddMessageError(EitherMandatoryMessage);
			}

			if (!Parent.CY_Code.IsEmpty && TransportMeans.Description.IsEmpty)
			{
				Parent.CY_CodeInfo.AddMessageError(Res.GetString("4F18334D-72DD-494F-A8B9-11312A2FC27E", "You have entered a vessel. Then its Agent Vessel Number must be entered. Please enter F3 in the Vessel Name column and enter its ID in the Agent Vessel Number field in the Vessel form."));
			}
		}

		public void ValidateDescription()
		{
			ValidateCalculatedProperty(TransportMeans.DescriptionInfo);
		}

		protected void CheckDescription()
		{
			if (!TransportMeans.Description.IsEmpty)
			{
				if (TransportMeans.Description.Length != 9)
				{
					TransportMeans.DescriptionInfo.AddMessageError(Res.GetString("EC6ED7AD-0F1C-4BEA-B081-876785B300FD", "The length of Vessel ID must be nine."));
				}
				else
				{
					var characters = new ZString[] { TransportMeans.Description.SubstringSafe(0, 3), TransportMeans.Description.SubstringSafe(3, 6) };
					ZInt outNumber = ZInt.Zero;
					if (Regex.Replace(characters[0], @"[^a-zA-Z]", "").Length != 3 || !ZInt.TryParse(characters[1], out outNumber))
					{
						TransportMeans.DescriptionInfo.AddMessageError(Res.GetString("CE4501A1-52DD-459D-B141-42CDB0C45116", "The first three characters must be English characters and remaining characters must be digits."));
					}
				}
			}
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (Parent.CY_Code == ZString.Empty && Parent.CY_Data == ZString.Empty)
			{
				Parent.CY_DataInfo.AddMessageError(EitherMandatoryMessage);
			}
		}

		public static string EitherMandatoryMessage => Res.GetString("1F45754E-23A2-4E38-8C87-F79F12D4BAF0", "Either Working Vessel Name or Transport Vehicle Reg No is Mandatory. Please enter one of the two fields.");

		TransportMeans TransportMeans => (TransportMeans)Parent;
	}
}
