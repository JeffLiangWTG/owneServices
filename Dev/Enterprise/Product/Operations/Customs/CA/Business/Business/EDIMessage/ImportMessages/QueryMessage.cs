using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class QueryMessage : EDIMessageWithBatchNumber
	{
		public QueryMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", ApplicationCodes.CAIMP).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.Query;
		}

		protected override string GetDocumentMessageVersion()
		{
			if (fDocumentMessageVersion.IsEmpty)
			{
				ZString numberFountainResult = "0000";
				while (numberFountainResult.Right(4) == "0000")
				{
					if (numberFountainResult.Length < 4)
					{
						throw new ArgumentException("Number fountain did not return a valid result");
					}

					numberFountainResult = Env.NumberFountains.EDIFACTNumberFountain("M", "VER", ApplicationCodes.CAIMP).GetNextFormatted(Factory).PadLeft(4, '0');
				}
				fDocumentMessageVersion = numberFountainResult.Right(4);
			}

			return fDocumentMessageVersion;
		}

		ZString fDocumentMessageVersion;

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			EM_ApplicationReference = GetDocumentMessageVersion();
		}

		#endregion

		#region Properties

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new QueryMessageSubType3CharCodes(); }
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		public override ZString DocumentMessageVersion
		{
			get
			{
				var result = string.Empty;
				if (EdifactMessage is Enterprise.Edifact.D99B.Messages.CUSDEC.CUSDECMessage)
				{
					var cusdec = (Enterprise.Edifact.D99B.Messages.CUSDEC.CUSDECMessage)EdifactMessage;
					if (cusdec.BGM.Count > 0)
					{
						result = cusdec.BGM[0].DocumentMessageIdentification.Version;
					}
				}
				else if (EdifactMessage is Enterprise.Edifact.D99B.Messages.CUSRES.CUSRESMessage)
				{
					var cusres = (Enterprise.Edifact.D99B.Messages.CUSRES.CUSRESMessage)EdifactMessage;
					if (cusres.BGM.Count > 0)
					{
						result = cusres.BGM[0].DocumentMessageIdentification.Version;
					}
				}
				return result;
			}
		}

		#endregion
	}
}
