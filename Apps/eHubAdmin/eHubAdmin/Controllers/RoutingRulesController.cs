using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using eServices.eHubDataModel.eHubTransactions;
using eServices.Shared.RoutingRuleEngine;
using Common.Logging;
using Common.Logging.Simple;

namespace eServices.eHubAdmin.Controllers
{
    public class RoutingRulesController : Controller
    {
        eHubTransactionsContext db;

        public RoutingRulesController() : this(new eHubTransactionsContext()) { }

        internal RoutingRulesController(eHubTransactionsContext db)
        {
            this.db = db;
        }

        public ActionResult Index()
        {
            var eHubClients = db.eHubClients.Where(c => c.CC_RR.HasValue);
            return View(eHubClients.ToList());
        }

        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            eHubClient eHubClient = db.eHubClients.Include(c => c.eHubRoutingRule).FirstOrDefault(c => c.CC_ID == id);
            if (eHubClient == null || eHubClient.eHubRoutingRule == null)
                return HttpNotFound();

            var rule = Rule.GetForReading(eHubClient);

            return View(rule);
        }

        public class TestLogger : AbstractSimpleLogger
        {
            internal List<Tuple<string, string>> Messages = new List<Tuple<string, string>>();

            public TestLogger(LogLevel level) : base("TestLogger", level, false, false, false, null) { }

            protected override void WriteInternal(LogLevel level, object message, Exception exception)
            {
                var sb = new StringBuilder();
                FormatOutput(sb, level, message, exception);
                Messages.Add(new Tuple<string, string>(level.ToString(), sb.ToString()));
            }
        }
    }
}
