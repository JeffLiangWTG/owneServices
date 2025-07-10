using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class CusPermitEnquiryMessage : GbEDIMessage
	{
		public CusPermitEnquiryMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CusPermitEnquiryMessage LinkedMessage
		{
			get
			{
				return HasLinkedMessage ? Factory.Load<CusPermitEnquiryMessage>(GetLinkedMessageQuery()).FirstOrDefault() : null;
			}
		}

		public ZBool HasLinkedMessage { get { return !EM_LinkUniqueID.IsEmpty && EM_LinkTable == CusPermitHeader.Schema.TableName; } }

		public ZString LinkedMessageInterpretation => LinkedMessage == null ? noResponsesForThisObjectHtmlMessage : (string)LinkedMessage.EM_MessageInterpretation;

		ZQuery GetLinkedMessageQuery()
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, this.PK);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, EDIMessageSchema.Constants.TableName);
			return query;
		}

		readonly string noResponsesForThisObjectHtmlMessage = "<html> <body style='font-family: arial;'> <p> No response message was found for this DLU. </p> </body> </html>";
	}
}
