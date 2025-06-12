import { ClipboardModule } from '@angular/cdk/clipboard';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from '../app-routing.module';
import { SharedModule } from '../shared/shared.module';
import { EndpointService } from '../shared/endpoint.service';
import { MessagesComponent } from './box/messages.component';
import { EditAutoSizeDirective } from './directives/autosize.directive';
import { AppStatusComponent } from './status/status.component';
import { UploaderComponent } from './uploader/uploader.component';
import { HelpComponent } from './info/info.component';

@NgModule({
	declarations: [
	  MessagesComponent,
	  AppStatusComponent,
	  UploaderComponent,
	  HelpComponent,
	  EditAutoSizeDirective,
	],
	imports: [
	  AppRoutingModule,
	  HttpClientModule,
	  ClipboardModule,
	  CommonModule,
	  SharedModule
	],
	providers: [EndpointService],
	bootstrap: [MessagesComponent]
  })

  export class MessageBoxModule { }