// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "must use db connection methods in audit db", Scope = "member", Target = "~M:Enterprise.AuditDataServices.ArchiveManager.Subscribers.DeleteOrphanSubscriber.ProcessChanges(Enterprise.Integration.ILogger,System.Collections.Generic.Dictionary{CargoWise.Schema.ITableSchema,System.Collections.Generic.List{System.String}})")]
