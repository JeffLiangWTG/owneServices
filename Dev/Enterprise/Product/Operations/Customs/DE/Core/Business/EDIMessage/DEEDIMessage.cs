using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public abstract class DEEDIMessage : EDIMessage
	{
		protected DEEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly EDIMessageTypeDecider TypeDecider = new EDIMessageTypeDecider();

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			return base.GetAdditionalRegisteredLinkedObjectTypes().Union(new Type[] { typeof(CusGuaranteeHeader) });
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var noteTypes = base.NoteTypesCore;
				noteTypes.Add(new PredefinedNoteType((NoResString)LogbookHelper.LogbookRegistrationNumberNoteDescription, StmNoteVisibility.INT, true, true, true, true));
				noteTypes.Add(new PredefinedNoteType((NoResString)LogbookHelper.LogbookLocalReferenceNumberNoteDescription, StmNoteVisibility.INT, true, true, true, true));
				noteTypes.Add(new PredefinedNoteType((NoResString)LogbookHelper.LogbookGUAMainAccessCode, StmNoteVisibility.INT, true, true, true, true));
				return noteTypes;
			}
		}

		protected override bool ClearMessageNumberOnFailureToSaveCore => true;
	}
}
