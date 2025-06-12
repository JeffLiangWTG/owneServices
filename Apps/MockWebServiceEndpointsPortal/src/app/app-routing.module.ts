import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MessagesComponent } from './messages/box/messages.component'
import { HelpComponent } from './messages/info/info.component'

const routes: Routes = [
  {path: 'messages/:id', component: MessagesComponent},
  {path: 'messages/:id/:subfolder', component: MessagesComponent},
  {path: 'info/:id', component: HelpComponent}
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes)
  ],
  exports: [RouterModule]
})
export class AppRoutingModule { }
