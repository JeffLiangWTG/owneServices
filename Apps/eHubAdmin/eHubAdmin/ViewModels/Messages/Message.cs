using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using eServices.eHubDataModel.eHubTransactions;
using Newtonsoft.Json;

namespace eServices.eHubAdmin.ViewModels.Messages
{
    public class Message
    {
        public eHubClient SenderObject { get; set; }
        public eHubClient RecipientObject { get; set; }

        public byte? Status { get; set; }
        public Guid? PK { get; set; }
        public Guid? AM_PK { get; set; }

        public string Subject { get; set; }

        public string Error { get; set; }

        public string GetMoreInfo(eHubClient client)
        {
	        if (client == null)
	        {
		        return null;
	        }

            var str = new StringBuilder();
            if (!string.IsNullOrEmpty(client.CC_FriendlyName))
            {
                str.Append("Name: ").AppendLine(client.CC_FriendlyName);
            }
            if (!string.IsNullOrEmpty(client.CC_OwnerCategory))
            {
                str.Append("Category: ").AppendLine(client.CC_OwnerCategory);
            }
            if (!string.IsNullOrEmpty(client.CC_SystemCategory))
            {
                str.Append("System: ").AppendLine(client.CC_SystemCategory);
            }
            return str.ToString();
        }
    }
}