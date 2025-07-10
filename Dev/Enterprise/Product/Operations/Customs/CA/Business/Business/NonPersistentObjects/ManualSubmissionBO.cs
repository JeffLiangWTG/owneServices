using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using IManualSubmissionSupport = Enterprise.Integration.Customs.CA.IManualSubmissionSupport;

namespace Enterprise.Customs.CA.Business
{
	public sealed class ManualSubmissionBO : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructor

		public ManualSubmissionBO(IManualSubmissionSupport support, string messageType, BusinessObjectFactory factory)
			: base(factory)
		{
			var manualSubmissionNoteText = support.ManualSubmissionNoteText;
			if (!manualSubmissionNoteText.IsEmpty)
			{
				var noteList = manualSubmissionNoteText.Replace("\r\n", "\r").Split('\r', '\n').ToList();
				while (noteList.Count >= 5)
				{
					var entrySubmissionBo = new CusEntrySubmissionBO(noteList[0], noteList.Take(5), factory);
					if (entrySubmissionBo.MessageType == messageType)
					{
						CurrentEntrySubmissionBO = entrySubmissionBo;
					}
					else
					{
						CusEntrySubmissionBOs.Add(entrySubmissionBo);
					}

					noteList.RemoveRange(0, 5);
					if (noteList.Count > 1)
					{
						noteList.RemoveAt(0);
					}
				}
			}

			if (CurrentEntrySubmissionBO == null)
			{
				CurrentEntrySubmissionBO = new CusEntrySubmissionBO(messageType, Array.Empty<ZString>(), factory);
			}
		}

		#endregion

		#region Properties

		public Integration.Customs.CA.IManualSubmissionNote[] GetCusEntrySubmissionBOs()
		{
			return fCusEntrySubmissionBOs.Cast<Integration.Customs.CA.IManualSubmissionNote>().ToArray();
		}

		List<CusEntrySubmissionBO> CusEntrySubmissionBOs
		{
			get
			{
				if (fCusEntrySubmissionBOs == null)
				{
					fCusEntrySubmissionBOs = new List<CusEntrySubmissionBO>();
				}
				return fCusEntrySubmissionBOs;
			}
		}
		List<CusEntrySubmissionBO> fCusEntrySubmissionBOs;

		[ChildEditable(true)]
		public CusEntrySubmissionBO CurrentEntrySubmissionBO
		{
			get
			{
				return fCurrentEntrySubmissionBO;
			}
			private set
			{
				if (fCurrentEntrySubmissionBO != null)
				{
					CusEntrySubmissionBOs.Remove(fCurrentEntrySubmissionBO);
					UnRegisterEditableChildObject(fCurrentEntrySubmissionBO);
				}
				else
				{
					fCurrentEntrySubmissionBO = value;
					if (fCurrentEntrySubmissionBO != null)
					{
						CusEntrySubmissionBOs.Add(fCurrentEntrySubmissionBO);
						RegisterEditableChildObject(fCurrentEntrySubmissionBO);
					}
				}
			}
		}

		CusEntrySubmissionBO fCurrentEntrySubmissionBO;

		#endregion
	}
}
