using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRImportDeclarationMessage : CMRCUSRESMessage
	{
		public CMRImportDeclarationMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference();
			if (!reference.IsEmpty && reference.Contains(CusEntryHeader.ReferenceNumberSeparator))
			{
				result = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, reference, EntryNumber);
			}
			if (result == null)
			{
				result = base.GetWrappedObject();
			}
			return result;
		}

		protected override ZString GetReportForFormattedMessage()
		{
			return ZString.Empty;
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (fEM_MessageInterpretation.IsEmpty && CUSRES != null)
				{
					fEM_MessageInterpretation = GetReport();
				}

				return fEM_MessageInterpretation;
			}
		}
		ZString fEM_MessageInterpretation;

		protected override List<MessageNotification> GetStatusNotifications()
		{
			List<MessageNotification> result = new List<MessageNotification>();

			if (CUSRES != null)
			{
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					if (group6.DOC.Count > 0)
					{
						ZString lineNumber = group6.DOC[0].DocumentMessageDetails.DocumentMessageNumber;
						ZString location = AdditionalPackLineReference(lineNumber);
						foreach (FTXSegment currentFTX in group6.FTX)
						{
							if (currentFTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails)
							{
								ZString lineStatusType = currentFTX.TextLiteral.FreeTextValue1;
								ZString lineStatusDescription = currentFTX.TextLiteral.FreeTextValue2;
								result.Add(new MessageNotification(location, lineStatusType, lineStatusDescription));
							}
						}
					}
				}
			}
			return result;
		}

		public override ZString LineStatusNotificationsCaption
		{
			get { return "Transport(Pack) Line status:"; }
		}

		ZString AdditionalPackLineReference(ZString lineNumber)
		{
			var result = ZString.Empty;
			ZShort line;
			if (ZShort.TryParse(lineNumber, out line) && line > 0)
			{
				var entryHeader = EM_LinkedObject as CusEntryHeader;
				if (entryHeader != null)
				{
					foreach (Package package in entryHeader.Packages)
					{
						if (package.PackingGroup != null && package.PackingGroup.CR_HouseContainerNumber == line)
						{
							result = (package.CW_HouseBill + " " + package.CW_ContainerNoOrEquipmentNo).TrimEnd();
							break;
						}
					}
				}
				if (result.IsEmpty)
				{
					result = "LINE " + lineNumber;
				}
			}
			return result;
		}
	}
}
