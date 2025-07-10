// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "BIJ - Use AetMinHistorySummaryLsn to get a list of changed tables - WI00601691", Scope = "member", Target = "~M:Enterprise.AuditDataServices.Subscription.SubscriberManager.HasChangesToProcess(Enterprise.AuditDataServices.Subscription.Common.IAuditSubscriberWrapper)~System.Boolean")] //C:\git\wtg\CargoWise\Dev\BusinessIntelligence\BiIntegration\Product\AuditDataServices\Subscription\SubscriberManager.cs:159:21
