using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.CA.Business
{
	public class EXPEDIMessage : EDIMessage
	{
		public EXPEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "EXP", ApplicationCodes.CAEXP).GetNextFormatted(Factory);
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.CAEXP;
		}

		protected override string GetEntryNumber()
		{
			var entryHeader = EM_LinkedObject as CusEntryHeader;
			if (entryHeader == null)
			{
				throw new ArgumentException("CusEntryHeader expected as Linked Object on an EXPEDImessage");
			}
			else
			{
				entryHeader.PopulateEntryNumberIfNeeded();
			}
			return entryHeader.EntryNumber;
		}

		#region Save

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				var entryHeader = EM_LinkedObject as CusEntryHeader;
				if (entryHeader != null && !entryHeader.CusEntryNumber.IsInDatabase)
				{
					entryHeader.EntryNumber = ZString.Empty;
				}
			}
		}
		#endregion
	}
}
