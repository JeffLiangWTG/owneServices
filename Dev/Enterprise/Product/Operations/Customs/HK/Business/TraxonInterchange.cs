using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Generic;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.HK.Business
{
	/// <summary>
	/// Summary description for TraxonEDIInterchange.
	/// </summary>
	public class TraxonInterchange : EDIInterchange
	{
		public TraxonInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override UNCharacterSet CharacterSet
		{
			get
			{
				UNCharacterSet charSet;
				if (EI_HeaderText.Contains("IATA"))
				{
					charSet = new UNOACharacterSet();
				}
				else
				{
					charSet = base.CharacterSet;
				}

				return charSet;
			}
		}

		protected override ZString GetInterchangeNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("I", EI_From, "Traxon").GetNextFormatted(Factory).ToUpper();
		}

		protected override string GetMessageNum(string messageText, int messageNumberSequece)
		{
			string uNHString = messageText.Substring(0, messageText.IndexOf(CharacterSet.SegmentDelimiterChar));
			try
			{
				UNHSegment segment = new UNHSegment();
				segment.Parse(CharacterSet, uNHString);
				return segment.CommonAccessReference;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return "";
			}
		}

		protected override bool ShouldSendViaEHubCore
		{
			get { return eHubMessagingRegistry.Instance.SendHKISACEViaEHub.Value; }
		}
	}
}
