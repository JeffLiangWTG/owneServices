using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCInterchange : EDIInterchange, Integration.Customs.AU.IEXDOCInterchange
	{
		public EXDOCInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString FormattedInterchangeForHumansToReadIt
		{
			get { return UNOACharacterSet.FromUNOB(EI_InterchangeText).Replace("'", "'" + System.Environment.NewLine); }
		}

		protected override void InitialiseValuesForIncomingInterchangeOnly(string eI_HeaderText, string eI_BodyText, string eI_FooterText, string eI_From, string eI_To, string eI_InterchangeNum, string eI_ApplicationCode)
		{
			// This is to swap the '8' and '7' values so that collisions are avoided due to duplicates.
			string oldFrom = eI_From;
			string oldTo = eI_To;
			eI_From = oldTo;
			eI_To = oldFrom;
			base.InitialiseValuesForIncomingInterchangeOnly(eI_HeaderText, eI_BodyText, eI_FooterText, eI_From, eI_To, eI_InterchangeNum, eI_ApplicationCode);
		}
	}
}
